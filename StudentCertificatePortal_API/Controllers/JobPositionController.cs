using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class JobPositionController : ApiControllerBase
    {
        private readonly IJobPositionService _jobPositionService;
        private readonly IMapper _mapper;
        public JobPositionController(IJobPositionService jobPositionService, IMapper mapper)
        {
            _jobPositionService = jobPositionService;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<Result<List<JobPositionDto>>>> GetAllJobPosition()
        {
            var result = await _jobPositionService.GetAll();
            return Ok(Result<List<JobPositionDto>>.Succeed(result));
        }
        [HttpGet("{jobPositionId:int}")]
        public async Task<ActionResult<Result<JobPositionDto>>> GetJobPositionById([FromRoute] int jobPositionId)
        {
            var result = await _jobPositionService.GetJobPositionByIdAsync(jobPositionId, new CancellationToken());
            return Ok(Result<JobPositionDto>.Succeed(result));
        }
        [HttpPost]
        public async Task<ActionResult<Result<JobPositionDto>>> CreateJobPosition([FromBody] CreateJobPositionRequest request)
        {
            var result = await _jobPositionService.CreateJobPositionAsync(request, new CancellationToken());
            return Ok(Result<JobPositionDto>.Succeed(result));
        }
        [HttpPut("{jobPositionId:int}")]
        public async Task<ActionResult<Result<JobPositionDto>>> UpdateJobPosition(int jobPositionId, UpdateJobPositionRequest request)
        {
            var result = await _jobPositionService.UpdateJobPositionAsync(jobPositionId, request, new CancellationToken());
            return Ok(Result<JobPositionDto>.Succeed(result));
        }
        [HttpDelete("{jobPositionId:int}")]
        public async Task<ActionResult<Result<JobPositionDto>>> DeleteJobPositionById([FromRoute] int jobPositionId)
        {
            var result = await _jobPositionService.DeleteJobPositionAsync(jobPositionId, new CancellationToken());
            return Ok(Result<JobPositionDto>.Succeed(result));
        }
        [HttpGet("{jobPositionName}")]
        public async Task<ActionResult<Result<List<JobPositionDto>>>> GetJobPositionByName([FromRoute] string jobPositionName)
        {
            var result = await _jobPositionService.GetJobPositionByNameAsync(jobPositionName, new CancellationToken());
            return Ok(Result<List<JobPositionDto>>.Succeed(result));
        }
        [HttpPut("Permission")]
        public async Task<ActionResult<Result<JobPositionDto>>> UpdateJobPositionPermission(int jobPositionId, [FromQuery] EnumPermission jobPositionPermission)
        {
            var result = await _jobPositionService.UpdateJobPositionPermissionAsync(jobPositionId, jobPositionPermission, new CancellationToken());
            return Ok(Result<JobPositionDto>.Succeed(result));
        }
    }
}
