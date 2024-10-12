using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class CertTypeController : ApiControllerBase
    {
        private readonly ICertTypeService _service;
        
        public CertTypeController(ICertTypeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<Result<List<CertTypeDto>>>> GetAllCertType()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<CertTypeDto>>.Succeed(result));
        }
        [HttpGet("{certTypeid:int}")]
        public async Task<ActionResult<Result<CertTypeDto>>> GetCertTypeById([FromRoute] int certTypeid)
        {
            var result = await _service.GetCertTypeByIdAsync(certTypeid, new CancellationToken());
            return Ok(Result<CertTypeDto>.Succeed(result));
        }

        [HttpPost]
        public async Task<ActionResult<Result<CertTypeDto>>> CreateCourse([FromBody] CreateCertTypeRequest request)
        {
            var result = await _service.CreateCertTypeAsync(request, new CancellationToken());
            return Ok(Result<CertTypeDto>.Succeed(result));
        }
        [HttpPut("{certTypeid:int}")]
        public async Task<ActionResult<Result<CertTypeDto>>> UpdateCertType(int certTypeid, UpdateCertTypeRequest request)
        {
            var result = await _service.UpdateCertTypeAsync(certTypeid, request, new CancellationToken());
            return Ok(Result<CertTypeDto>.Succeed(result));
        }
        [HttpDelete("{certTypeid:int}")]
        public async Task<ActionResult<Result<CourseDto>>> DeleteCourseById([FromRoute] int certTypeid)
        {
            var result = await _service.DeleteCertTypeAsync(certTypeid, new CancellationToken());
            return Ok(Result<CertTypeDto>.Succeed(result));
        }
        [HttpGet("~/api/v1/[controller]/search")]
        public async Task<ActionResult<Result<List<CertTypeDto>>>> GetCourseByName([FromQuery] string? certTypeName)
        {
            var result = new List<CertTypeDto>();
            if (certTypeName == null)
            {
                result = await _service.GetAll();
            }
            else
            {
                result = await _service.GetCertTypeByNameAsync(certTypeName, new CancellationToken());
            }
             
            return Ok(Result<List<CertTypeDto>>.Succeed(result));
        }
    }
}
