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

namespace StudentCertificatePortal_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GoogleController: ControllerBase
    {
        private readonly IUnitOfWork _uow;
        private readonly GenerateJSONWebTokenHelper _helper;
        private readonly ILoginService _service;
        public GoogleController(IUnitOfWork uow, GenerateJSONWebTokenHelper helper, ILoginService service)
        {
            _uow = uow;
            _helper = helper;
            _service = service;
        }

        [HttpGet("login-google")]
        public IActionResult LoginGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Google");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse(CancellationToken cancellationToken)
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
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
                    Role = "Guest",
                    Status = true,
                    UserCreatedAt = DateTime.Now,
                };
                await _uow.UserRepository.AddAsync(user);
                await _uow.Commit(cancellationToken);
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, result.Principal);

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



    }
}
