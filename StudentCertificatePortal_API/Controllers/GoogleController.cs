using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_Data.Models;
using System.Security.Claims;
using StudentCertificatePortal_Repository.Interface;
using StudentCertificatePortal_API.Utils;
using Azure.Core;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Responses;
using Microsoft.AspNetCore.Identity.Data;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.Services.Implemetation;
using StudentCertificatePortal_API.Services.Interface;
using Microsoft.SqlServer.Server;
using Google.Apis.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AutoMapper;

namespace StudentCertificatePortal_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GoogleController : ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly GenerateJSONWebTokenHelper _helper;
        private readonly ILoginService _service;
        private readonly IUserService _userService;
        private readonly IWalletService _walletService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public GoogleController(IUnitOfWork uow, GenerateJSONWebTokenHelper helper, 
            ILoginService service, IWalletService walletService
            , IConfiguration configuration,
            IUserService userService, IMapper mapper)
        {
            _uow = uow;
            _helper = helper;
            _service = service;
            _walletService = walletService;
            _configuration = configuration;
            _userService = userService;
            _mapper = mapper;

        }

        [HttpPost("{IdToken}")]
        public async Task<IActionResult> GoogleSignIn([FromRoute] string IdToken)
        {
            try
            {
                // Validate Google ID Token
                var payload = await GoogleJsonWebSignature.ValidateAsync(IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                });

                if (payload == null)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Invalid Google token."
                    });
                }

                var email = payload.Email;
                var username = payload.Name;
                var userImage = payload.Picture;

                // Check if user exists in the database
                var existingUser = await _uow.UserRepository.FirstOrDefaultAsync(x => x.Email == email);

                // If user does not exist, create a new user
                if (existingUser == null)
                {
                    var request = new CreateUserRequest
                    {
                        Username = username,
                        Email = email,
                        UserImage = userImage
                    };

                    var newUser = await _userService.CreateUserAsync(request, new CancellationToken());

                    // Generate JWT Token for new user
                    var token = GenerateJwtToken(_mapper.Map<User>(newUser));

                    return Ok(new
                    {
                        Success = true,
                        Message = "User registered successfully.",
                        Token = token,
                        Data = new
                        {
                            newUser.Email,
                            newUser.Fullname,
                            newUser.UserImage
                        }
                    });
                }

                // Generate JWT Token for existing user
                var existingToken = GenerateJwtToken(existingUser);

                return Ok(new
                {
                    Success = true,
                    Message = "User logged in successfully.",
                    Token = existingToken,
                    Data = new
                    {
                        existingUser.Email,
                        existingUser.Fullname,
                        existingUser.UserImage
                    }
                });
            }
            catch (InvalidJwtException ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = $"Invalid Google token: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"An error occurred: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Generates a JWT token for the user.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <returns>A JWT token string.</returns>
        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Fullname),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim("UserId", user.UserId.ToString())
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        /*[HttpGet("login-google")]
        public IActionResult LoginGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Google");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<GoogleTokenPayload> GoogleResponse(string idToken, CancellationToken cancellationToken)
        {

                try
                {
                    // Ensure token is correctly formatted
                    if (string.IsNullOrWhiteSpace(idToken) ||
                        !idToken.Contains('.') ||
                        idToken.Split('.').Length != 3)
                    {
                        return null;
                    }

                    // Decode the token
                    var handler = new JwtSecurityTokenHandler();
                    var token = handler.ReadJwtToken(idToken);

                    return new GoogleTokenPayload
                    {
                        Sub = token.Claims.FirstOrDefault(c => c.Type == "sub")?.Value,
                        Email = token.Claims.FirstOrDefault(c => c.Type == "email")?.Value,
                        GivenName = token.Claims.FirstOrDefault(c => c.Type == "given_name")?.Value,
                        FamilyName = token.Claims.FirstOrDefault(c => c.Type == "family_name")?.Value
                    };
                }
                catch
                {
                    return null;
                }*/


            /*var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            var claims = result.Principal?.Identities.FirstOrDefault()?.Claims;

            if (claims == null)
            {
                return BadRequest("Failed to receive authentication information from Google.");
            }

            // Lấy các thông tin từ claims
            var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var firstName = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
            var lastName = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

            var user = await _uow.UserRepository.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                user = new User
                {
                    Username = !string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName)
                        ? $"{firstName} {lastName}"
                        : "GuestUser",
                    Password = string.Empty, 
                    Email = email,
                    Fullname = !string.IsNullOrWhiteSpace(firstName) && !string.IsNullOrWhiteSpace(lastName)
                        ? $"{firstName} {lastName}"
                        : "Guest User",
                    Dob = DateTime.Now,
                    Address = "Student Certificate Portal",
                    PhoneNumber = "0123456789",
                    Role = "Student",
                    Status = true,
                    UserCreatedAt = DateTime.Now,
                };
                var resultUser = await _uow.UserRepository.AddAsync(user);
                await _uow.Commit(cancellationToken);
                var walletResult = await _walletService.CreateWalletAsync(resultUser.UserId, new CancellationToken());
                if (walletResult == null)
                {
                    return BadRequest("Wallet was not successfully created");
                }*/
            /*}*/



            /*await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal);

            var request = new LoginUserRequest { Email = user.Email, Password = user.Password };
            var userToken = await _service.Authenticate(request, cancellationToken);

            if (userToken == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            // Tạo JWT token
            var token = _helper.GenerateJSONWebToken(userToken);

            return Ok(new
            {
                Message = "Login successful.",
                Token = token
            });
        }
*/


        }
/*
        public class GoogleTokenPayload
        {
            public string Sub { get; set; }
            public string Email { get; set; }
            public string GivenName { get; set; }
            public string FamilyName { get; set; }
            public string Picture { get; set; }
        }*/

}
