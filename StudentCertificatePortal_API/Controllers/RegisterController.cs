using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Filters.ActionFilters;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class RegisterController : ApiControllerBase
    {
        private readonly IUserService _service;

        public RegisterController(IUserService service)
        {
            _service = service;
        }

        [HttpPost]
        [ValidateRequest(typeof(CreateRegisterUserRequest))]
        public async Task<ActionResult<Result<UserDto>>> CreateUser([FromBody] CreateRegisterUserRequest request)
        {
            var result = await _service.CreateRegisterUserAsync(request, new CancellationToken());
            return Ok(Result<UserDto>.Succeed(result));
        }
    }
}
