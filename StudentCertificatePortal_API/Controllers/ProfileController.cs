using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Filters.ActionFilters;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class ProfileController: ApiControllerBase
    {
        private readonly IProfileService _service;

        public ProfileController(IProfileService service)
        {
            _service = service;
        }

        [HttpGet("{userId:int}")]
        public async Task<ActionResult<Result<UserDto>>> GetProfileById([FromRoute] int userId)
        {
            var result = await _service.GetProfileByIdAsync(userId, new CancellationToken());
            return Ok(Result<UserDto>.Succeed(result));
        }
        [HttpPut("{userId:int}")]
        [ValidateRequest(typeof(UpdateProfileRequest))]
        public async Task<ActionResult<Result<UserDto>>> UpdateProfileUser(int userId, UpdateProfileRequest request)
        {
            var result = await _service.UpdateProfileAsync(userId, request, new CancellationToken());
            return Ok(Result<UserDto>.Succeed(result));
        }
    }
}
