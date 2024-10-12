using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class WalletController: ApiControllerBase
    {
        private readonly IWalletService _service;

        public WalletController(IWalletService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<Result<List<WalletDto>>>> GetAllWallet()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<WalletDto>>.Succeed(result));
        }

        [HttpGet("{userId:int}")]
        public async Task<ActionResult<Result<CourseDto>>> GetWalletById([FromRoute] int userId)
        {
            var result = await _service.GetWalletByIdAsync(userId, new CancellationToken());
            return Ok(Result<WalletDto>.Succeed(result));
        }

        [HttpPost("{userId:int}")]
        public async Task<ActionResult<Result<WalletDto>>> CreateWallet([FromRoute] int userId)
        {
            var result = await _service.CreateWalletAsync(userId, new CancellationToken());
            return Ok(Result<WalletDto>.Succeed(result));
        }
        [HttpPut("{userId:int}")]
        public async Task<ActionResult<Result<CourseDto>>> UpdateCourse(int userId, [FromQuery] int point,[FromQuery] Enums.EnumWallet status )
        {
            var result = await _service.UpdateWalletAsync(userId, point, status, new CancellationToken());
            return Ok(Result<WalletDto>.Succeed(result));
        }
        [HttpDelete("{userId:int}")]
        public async Task<ActionResult<Result<WalletDto>>> DeleteCourseById([FromRoute] int userId)
        {
            var result = await _service.DeleteWalletAsync(userId, new CancellationToken());
            return Ok(Result<WalletDto>.Succeed(result));
        }
    }
}
