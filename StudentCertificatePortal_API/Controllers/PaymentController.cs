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

        [HttpGet("{paymentId:int}")]
        public async Task<ActionResult<Result<PaymentDto>>> GetPaymentById([FromRoute] int paymentId)
        {
            var result = await _service.GetPaymentByIdAsync(paymentId);
            return Ok(Result<PaymentDto>.Succeed(result));
        }

        [HttpGet("get-ExamEnrollment-by-userId/{userId:int}")]
        public async Task<ActionResult<Result<List<PaymentDto>>>> GetPaymentOfExamEnrollmentById([FromRoute] int userId)
        {
            var result = await _service.GetEEnrollPaymentByUserIdAsync(userId);
            return Ok(Result<List<PaymentDto>>.Succeed(result));
        }

        [HttpGet("get-CourseEnrollment-by-userId/{userId:int}")]
        public async Task<ActionResult<Result<List<PaymentDto>>>> GetPaymentOfCourseEnrollmentById([FromRoute] int userId)
        {
            var result = await _service.GetCEnrollPaymentByUserIdAsync(userId);
            return Ok(Result<List<PaymentDto>>.Succeed(result));
        }

        [HttpPost]
        public async Task<ActionResult<Result<PaymentDto>>> ProcessPayment([FromBody] CreatePaymentRequest request)
        {
            var result = await _service.ProcessPayment(request, new CancellationToken());
            return Ok(Result<PaymentDto>.Succeed(result));
        }
    }
}
