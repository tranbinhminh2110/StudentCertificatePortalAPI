using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class PaymentController : ApiControllerBase
    {
        private readonly IPaymentService _service;
        private readonly IExamEnrollmentService _examEnrollmentService;
        private readonly ICourseEnrollmentService _courseEnrollmentService;

        public PaymentController(IPaymentService service, IExamEnrollmentService examEnrollmentService, ICourseEnrollmentService courseEnrollmentService)
        {
            _service = service;
            _examEnrollmentService = examEnrollmentService;
            _courseEnrollmentService = courseEnrollmentService;
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

        [HttpPost("pay-now")]
        public async Task<ActionResult<Result<PaymentDto>>> PayNow([FromBody] CreatePayNowRequest request)
        {

            if (request.Simulation_Exams.Any(id => id != 0 && request.UserId != 0))
            {

                var enroll = new CreateExamEnrollmentVoucherRequest()
                {
                    UserId = request.UserId,
                    Simulation_Exams = request.Simulation_Exams,
                    VoucherIds = request.VoucherIds,
                };
                var eEnrollment = await _examEnrollmentService.CreateExamEnrollmentVoucherAsync(enroll, new CancellationToken());
                if (eEnrollment == null) throw new Exception("Error enrollment and please try again.");
                var payment = new CreatePaymentRequest()
                {
                    UserId = enroll.UserId,
                    ExamEnrollmentId = eEnrollment.ExamEnrollmentId,
                };
                var payNow = await _service.ProcessPayment(payment, new CancellationToken());
                if (payNow == null) throw new Exception("Error payment and please try again.");
                return Ok(Result<PaymentDto>.Succeed(payNow));
            }
            else if (request.Courses.Any(id => id != 0 && request.UserId != 0))
            {
                var enroll = new CreateCourseEnrollmentRequest()
                {
                    UserId = request.UserId,
                    Courses = request.Courses,
                };

                var cEnrollment = await _courseEnrollmentService.CreateCourseEnrollmentAsync(enroll, new CancellationToken());
                if (cEnrollment == null) throw new Exception("Error enrollment and please try again.");
                var payment = new CreatePaymentRequest()
                {
                    UserId = request.UserId,
                    CourseEnrollmentId = cEnrollment.CourseEnrollmentId,
                };
                var payNow = await _service.ProcessPayment(payment, new CancellationToken());
                if (payNow == null) throw new Exception("Error payment and please try again.");
                return Ok(Result<PaymentDto>.Succeed(payNow));
            }

            return BadRequest("Error processing payment.");

        }
    }
}
