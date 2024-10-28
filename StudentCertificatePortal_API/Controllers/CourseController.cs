using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class CourseController : ApiControllerBase
    {
        private readonly ICourseService _service;
        private readonly IMapper _mapper;

        public CourseController(ICourseService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<Result<List<CourseDto>>>> GetAllCourse()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<CourseDto>>.Succeed(result));
        }
        [HttpGet("{courseId:int}")]
        public async Task<ActionResult<Result<CourseDto>>> GetCourseById([FromRoute] int courseId)
        {
            var result = await _service.GetCourseByIdAsync(courseId, new CancellationToken());
            return Ok(Result<CourseDto>.Succeed(result));
        }
        
        [HttpPost]
        public async Task<ActionResult<Result<CourseDto>>> CreateCourse([FromBody] CreateCourseRequest request)
        {
            var result = await _service.CreateCourseAsync(request, new CancellationToken());
            return Ok(Result<CourseDto>.Succeed(result));
        }
        [HttpPut("{courseId:int}")]
        public async Task<ActionResult<Result<CourseDto>>> UpdateCourse(int courseId, UpdateCourseRequest request)
        {
            var result = await _service.UpdateCourseAsync(courseId, request, new CancellationToken());
            return Ok(Result<CourseDto>.Succeed(result));
        }
        [HttpDelete("{courseId:int}")]
        public async Task<ActionResult<Result<CourseDto>>> DeleteCourseById([FromRoute] int courseId)
        {
            var result = await _service.DeleteCourseAsync(courseId, new CancellationToken());
            return Ok(Result<CourseDto>.Succeed(result));
        }
        [HttpGet("{courseName}")]
        public async Task<ActionResult<Result<List<CourseDto>>>> GetCourseByName([FromRoute] string courseName)
        {
            var result = await _service.GetCourseByNameAsync(courseName, new CancellationToken());
            return Ok(Result<List<CourseDto>>.Succeed(result));
        }
        [HttpPut("Permission")]
        public async Task<ActionResult<Result<CourseDto>>> UpdateCoursePermission(int courseId, [FromQuery] EnumPermission coursePermission)
        {
            var result = await _service.UpdateCoursePermissionAsync(courseId, coursePermission, new CancellationToken());
            return Ok(Result<CourseDto>.Succeed(result));
        }
    }
}
