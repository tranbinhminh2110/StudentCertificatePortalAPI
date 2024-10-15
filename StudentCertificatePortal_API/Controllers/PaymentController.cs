using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class PaymentController: ApiControllerBase
    {
        private readonly IPaymentService _service;

        public PaymentController(IPaymentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<Result<List<PaymentDto>>>> GetAllPayment()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<PaymentDto>>.Succeed(result));
        }

        [HttpGet("{certId:int}")]
        public async Task<ActionResult<Result<PaymentDto>>> GetCertificationById([FromRoute] int certId)
        {
            var result = await _service.GetPaymentByIdAsync(certId);
            return Ok(Result<PaymentDto>.Succeed(result));
        }

        [HttpPost]
        public async Task<ActionResult<Result<PaymentDto>>> ProcessPayment([FromBody] CreatePaymentRequest request)
        {
            var result = await _service.ProcessPayment(request, new CancellationToken());
            return Ok(Result<PaymentDto>.Succeed(result));
        }
    }
}
