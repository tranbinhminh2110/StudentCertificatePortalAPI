using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Filters.ActionFilters;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class UsersController : ApiControllerBase
    {
        private readonly IUserService _service;
        private readonly IMapper _mapper;

        public UsersController(IUserService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<Result<List<UserDto>>>> GetAllUser()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<UserDto>>.Succeed(result));
        }
        [HttpGet("{userId:int}")]
        public async Task<ActionResult<Result<UserDto>>> GetUserById([FromRoute] int userId)
        {
            var result = await _service.GetUserByIdAsync(userId, new CancellationToken());
            return Ok(Result<UserDto>.Succeed(result));
        }

        [HttpPost]
        [ValidateRequest(typeof(CreateUserRequest))]
        public async Task<ActionResult<Result<UserDto>>> CreateUser([FromBody] CreateUserRequest request)
        {
            var result = await _service.CreateUserAsync(request, new CancellationToken());
            return Ok(Result<UserDto>.Succeed(result));
        }

        [HttpPut("{userId:int}")]
        [ValidateRequest(typeof(UpdateUserRequest))]
        public async Task<ActionResult<Result<UserDto>>> UpdateUser(int userId, UpdateUserRequest request)
        {
            var result = await _service.UpdateUserAsync(userId, request, new CancellationToken());
            return Ok(Result<UserDto>.Succeed(result));
        }

        [HttpDelete("{userId:int}")]
        public async Task<ActionResult<Result<UserDto>>> DeleteUserById([FromRoute] int userId)
        {
            var result = await _service.DeleteUserByIdAsync(userId, new CancellationToken());
            return Ok(Result<UserDto>.Succeed(result));
        }

        [HttpPost("{userId:int}/setstatus")]
        public async Task<ActionResult<Result<UserDto>>> ChangeStatusAccount([FromRoute] int userId)
        {
            var result = await _service.ChangeStatusAccountAsync(userId, new CancellationToken());
            return Ok(Result<UserDto>.Succeed(result));
        }

    }
}
