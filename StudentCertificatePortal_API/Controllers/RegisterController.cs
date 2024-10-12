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
        private readonly IWalletService _walletService;

        public RegisterController(IUserService service, IWalletService walletService)
        {
            _service = service;
            _walletService = walletService;
        }

        [HttpPost]
        [ValidateRequest(typeof(CreateRegisterUserRequest))]
        public async Task<ActionResult<Result<UserDto>>> CreateUser([FromBody] CreateRegisterUserRequest request)
        {
            var result = await _service.CreateRegisterUserAsync(request, new CancellationToken());
            var walletResult = await _walletService.CreateWalletAsync(result.UserId, new CancellationToken());
            if(walletResult == null)
            {
                return BadRequest("Wallet was not successfully created");
            }
            return Ok(Result<UserDto>.Succeed(result));
        }
    }
}
