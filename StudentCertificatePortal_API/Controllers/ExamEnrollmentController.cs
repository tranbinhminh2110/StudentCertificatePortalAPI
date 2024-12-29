using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.Contracts.Responses;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class ExamEnrollmentController: ApiControllerBase
    {
        private readonly IExamEnrollmentService _service;
        public ExamEnrollmentController(IExamEnrollmentService service) {
            _service = service;
        
        }
        [HttpGet]
        public async Task<ActionResult<Result<List<ExamEnrollmentDto>>>> GetAllExamEnrollment()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<ExamEnrollmentDto>>.Succeed(result));
        }
        [HttpGet("{eEnrollmentId:int}")]
        public async Task<ActionResult<Result<ExamEnrollmentDto>>> GetExamEnrollmentById([FromRoute] int eEnrollmentId)
        {
            var result = await _service.GetExamEnrollmentById(eEnrollmentId, new CancellationToken());
            return Ok(Result<ExamEnrollmentDto>.Succeed(result));
        }

        [HttpGet("get-by-userId/{userId:int}")]
        public async Task<ActionResult<Result<List<ExamEnrollmentDto>>>> GetExamEnrollmentByUserId([FromRoute] int userId)
        {
            var result = await _service.GetExamEnrollmentByUserId(userId, new CancellationToken());
            return Ok(Result<List<ExamEnrollmentDto>>.Succeed(result));
        }
        [HttpPost]
        public async Task<ActionResult<Result<ExamEnrollmentResponse>>> CreateExamEnrollment([FromBody] CreateExamEnrollmentRequest request)
        {
            var result = await _service.CreateExamEnrollmentAsync(request, new CancellationToken());
            return Ok(Result<ExamEnrollmentResponse>.Succeed(result));
        }
        [HttpPut("{eEnrollmentId:int}")]
        public async Task<ActionResult<Result<ExamEnrollmentDto>>> UpdateExamEnrollment(int eEnrollmentId, UpdateExamEnrollmentRequest request)
        {
            var result = await _service.UpdateExamEnrollmentAsync(eEnrollmentId, request, new CancellationToken());
            return Ok(Result<ExamEnrollmentDto>.Succeed(result));
        }

        /*[HttpGet("~/api/v1/[controller]/search")]
        public async Task<ActionResult<Result<List<CertificationDto>>>> GetCertificcationByName([FromQuery] string? certName = null)
        {
            IEnumerable<CertificationDto> result;
            if (certName == null)
            {
                result = await _service.GetAll();
            }
            else
            {
                result = await _service.GetCertificationByNameAsync(certName, new CancellationToken());
            }

            return Ok(Result<List<CertificationDto>>.Succeed(result.ToList()));
        }*/
        [HttpDelete("{eEnrollmentId:int}")]
        public async Task<ActionResult<Result<ExamEnrollmentDto>>> DeleteCertificationById([FromRoute] int eEnrollmentId)
        {
            var result = await _service.DeleteExamEnrollmentAsync(eEnrollmentId, new CancellationToken());
            return Ok(Result<ExamEnrollmentDto>.Succeed(result));
        }

        [HttpPost]
        [Route("create-with-voucher")]
        public async Task<ActionResult<Result<ExamEnrollmentResponse>>> CreateExamEnrollmentVoucher([FromBody] CreateExamEnrollmentVoucherRequest request)
        {
            var result = await _service.CreateExamEnrollmentVoucherAsync(request, new CancellationToken());
            return Ok(Result<ExamEnrollmentResponse>.Succeed(result));
        }

        [HttpPut("update-with-voucher/{eEnrollmentId:int}")]
        public async Task<ActionResult<Result<ExamEnrollmentDto>>> UpdateExamEnrollmentVoucher(int eEnrollmentId, UpdateExamEnrollmentVoucherRequest request)
        {
            var result = await _service.UpdateExamEnrollmentVoucherAsync(eEnrollmentId, request, new CancellationToken());
            return Ok(Result<ExamEnrollmentDto>.Succeed(result));
        }

    }
}
