using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class ProfileController: ApiControllerBase
    {
        private readonly IUserService _service;

        public ProfileController(IUserService service)
        {
            _service = service;
        }

        [HttpGet("{userId:int}")]
        public async Task<ActionResult<Result<UserDto>>> GetUserById([FromRoute] int userId)
        {
            var result = await _service.GetUserByIdAsync(userId, new CancellationToken());
            return Ok(Result<UserDto>.Succeed(result));
        }
    }
}
