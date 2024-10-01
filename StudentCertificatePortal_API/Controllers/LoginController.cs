using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.Contracts.Responses;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;
using System.Security.Claims;

namespace StudentCertificatePortal_API.Controllers
{
    public class LoginController: ApiControllerBase
    {
        public readonly ILoginService _service;
        private readonly GenerateJSONWebTokenHelper _helper;

        public LoginController(ILoginService service, GenerateJSONWebTokenHelper helper)
        {
            _service = service;
            _helper = helper;
        }
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginUserRequest request)
        {
            var user = await _service.Authenticate(request, new CancellationToken());
            if (user != null)
            {
                var tokenString = _helper.GenerateJSONWebToken(user);
                var result = new LoginSuccessResponse
                {
                    AccessToken = tokenString,
                };

                return Ok(Result<LoginSuccessResponse>.Succeed(result));
            }
            else
            {
                return Unauthorized("Email or Password is incorrect!");
            }
        }

        
    }
}
