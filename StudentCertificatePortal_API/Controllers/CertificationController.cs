using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class CertificationController: ApiControllerBase
    {
        private readonly ICertificationService _service;
        private readonly IMapper _mapper;

        public CertificationController(ICertificationService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<Result<List<CertificationDto>>>> GetAllCertification()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<CertificationDto>>.Succeed(result));
        }
        [HttpGet("{certId:int}")]
        public async Task<ActionResult<Result<CertificationDto>>> GetCertificationById([FromRoute] int certId)
        {
            var result = await _service.GetCertificationById(certId, new CancellationToken());
            return Ok(Result<CertificationDto>.Succeed(result));
        }
        [HttpPost]
        public async Task<ActionResult<Result<CertificationDto>>> CreateCertification([FromBody] CreateCertificationRequest request)
        {
            var result = await _service.CreateCertificationAsync(request, new CancellationToken());
            return Ok(Result<CertificationDto>.Succeed(result));
        }
        [HttpPut("{certId:int}")]
        public async Task<ActionResult<Result<CertificationDto>>> UpdateCertification(int certId, UpdateCertificationRequest request)
        {
            var result = await _service.UpdateCertificationAsync(certId, request, new CancellationToken());
            return Ok(Result<CertificationDto>.Succeed(result));
        }
        /*[HttpDelete("{certId:int}")]
        public async Task<ActionResult<Result<CourseDto>>> DeleteCertificationById([FromRoute] int certId)
        {
            var result = await _service.DeleteCertificationAsync(certId, new CancellationToken());
            return Ok(Result<CertificationDto>.Succeed(result));
        }*/
    }
}
