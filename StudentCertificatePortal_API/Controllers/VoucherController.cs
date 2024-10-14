using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class VoucherController : ApiControllerBase
    {
        private readonly IVoucherService _service;
        private readonly IMapper _mapper;

        public VoucherController(IVoucherService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<Result<List<VoucherDto>>>> GetAllVoucher()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<VoucherDto>>.Succeed(result));
        }
        [HttpGet("{voucherId:int}")]
        public async Task<ActionResult<Result<VoucherDto>>> GetVoucherById([FromRoute] int voucherId)
        {
            var result = await _service.GetVoucherByIdAsync(voucherId, new CancellationToken());
            return Ok(Result<VoucherDto>.Succeed(result));
        }
        [HttpPost]
        public async Task<ActionResult<Result<VoucherDto>>> CreateVoucher([FromBody] CreateVoucherRequest request)
        {
            var result = await _service.CreateVoucherAsync(request, new CancellationToken());
            return Ok(Result<VoucherDto>.Succeed(result));
        }
        [HttpPut("{voucherId:int}")]
        public async Task<ActionResult<Result<VoucherDto>>> UpdateVoucher(int voucherId, UpdateVoucherRequest request)
        {
            var result = await _service.UpdateVoucherAsync(voucherId, request, new CancellationToken());
            return Ok(Result<VoucherDto>.Succeed(result));
        }
        [HttpDelete("{voucherId:int}")]
        public async Task<ActionResult<Result<VoucherDto>>> DeleteVoucherById([FromRoute] int voucherId)
        {
            var result = await _service.DeleteVoucherAsync(voucherId, new CancellationToken());
            return Ok(Result<VoucherDto>.Succeed(result));
        }
        [HttpGet("{voucherName}")]
        public async Task<ActionResult<Result<List<VoucherDto>>>> GetVoucherByName([FromRoute] string voucherName)
        {
            var result = await _service.GetVoucherByNameAsync(voucherName, new CancellationToken());
            return Ok(Result<List<VoucherDto>>.Succeed(result));
        }
    }
}
