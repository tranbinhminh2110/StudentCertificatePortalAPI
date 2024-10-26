using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class CourseEnrollmentController : ApiControllerBase
    {
        private readonly ICourseEnrollmentService _service;
        private readonly IMapper _mapper;

        public CourseEnrollmentController(ICourseEnrollmentService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<Result<List<CourseEnrollmentDto>>>> GetAllCourse()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<CourseEnrollmentDto>>.Succeed(result));
        }

        [HttpGet("{courseEnrollmentId:int}")]
        public async Task<ActionResult<Result<CourseEnrollmentDto>>> GetCourseEnrollmentById([FromRoute] int courseEnrollmentId)
        {
            var result = await _service.GetCourseEnrollmentByIdAsync(courseEnrollmentId, new CancellationToken());
            return Ok(Result<CourseEnrollmentDto>.Succeed(result));
        }

        [HttpPost]
        public async Task<ActionResult<Result<CourseEnrollmentDto>>> CreateCourseEnrollment([FromBody] CreateCourseEnrollmentRequest request)
        {
            var result = await _service.CreateCourseEnrollmentAsync(request, new CancellationToken());
            return Ok(Result<CourseEnrollmentDto>.Succeed(result));
        }
        [HttpPut("{courseEnrollmentId:int}")]
        public async Task<ActionResult<Result<CourseEnrollmentDto>>> UpdateCourseEnrollment(int courseEnrollmentId, UpdateCourseEnrollmentRequest request)
        {
            var result = await _service.UpdateCourseEnrollmentAsync(courseEnrollmentId, request, new CancellationToken());
            return Ok(Result<CourseEnrollmentDto>.Succeed(result));
        }
        [HttpDelete("{courseEnrollmentId:int}")]
        public async Task<ActionResult<Result<CourseEnrollmentDto>>> DeleteCourseEnrollmentById([FromRoute] int courseEnrollmentId)
        {
            var result = await _service.DeleteCourseEnrollmentAsync(courseEnrollmentId, new CancellationToken());
            return Ok(Result<CourseEnrollmentDto>.Succeed(result));
        }
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<Result<List<CourseEnrollmentDto>>>> GetCourseEnrollmentByUserId([FromRoute] int userId)
        {
            var result = await _service.GetCourseEnrollmentByUserIdAsync(userId, new CancellationToken());
            return Ok(Result<List<CourseEnrollmentDto>>.Succeed(result));
        }


    }
}
