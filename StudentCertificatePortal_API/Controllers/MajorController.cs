using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Services.Interface;
using System.Security;

namespace StudentCertificatePortal_API.Controllers
{
    public class MajorController : ApiControllerBase
    {
        private readonly IMajorService _service;
        private readonly IMapper _mapper;

        public MajorController(IMajorService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<Result<List<MajorDto>>>> GetAllMajor()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<MajorDto>>.Succeed(result));
        }
        [HttpGet("{majorId:int}")]
        public async Task<ActionResult<Result<MajorDto>>> GetMajorById([FromRoute] int majorId)
        {
            var result = await _service.GetMajorByIdAsync(majorId, new CancellationToken());
            return Ok(Result<MajorDto>.Succeed(result));
        }
        [HttpPost]
        public async Task<ActionResult<Result<MajorDto>>> CreateMajor([FromBody] CreateMajorRequest request)
        {
            var result = await _service.CreateMajorAsync(request, new CancellationToken());
            return Ok(Result<MajorDto>.Succeed(result));
        }
        [HttpPut("{majorId:int}")]
        public async Task<ActionResult<Result<MajorDto>>> UpdateMajor(int majorId, UpdateMajorRequest request)
        {
            var result = await _service.UpdateMajorAsync(majorId, request, new CancellationToken());
            return Ok(Result<MajorDto>.Succeed(result));
        }
        [HttpDelete("{majorId:int}")]
        public async Task<ActionResult<Result<MajorDto>>> DeleteMajorById([FromRoute] int majorId)
        {
            var result = await _service.DeleteMajorAsync(majorId, new CancellationToken());
            return Ok(Result<MajorDto>.Succeed(result));
        }
        [HttpGet("{majorName}")]
        public async Task<ActionResult<Result<List<MajorDto>>>> GetMajorByName([FromRoute] string majorName)
        {
            var result = await _service.GetMajorByNameAsync(majorName, new CancellationToken());
            return Ok(Result<List<MajorDto>>.Succeed(result));
        }
        [HttpPut("Permission")]
        public async Task<ActionResult<Result<MajorDto>>> UpdateMajorPermission(int majorId, [FromQuery] EnumPermission majorPermission)
        {
            var result = await _service.UpdateMajorPermissionAsync(majorId, majorPermission, new CancellationToken());
            return Ok(Result<MajorDto>.Succeed(result));
        }
        [HttpGet("{majorId:int}/jobPosition/{jobPositionId:int}")]
        public async Task<ActionResult<Result<List<MajorDto>>>> GetMajorListById(int majorId, int jobPositionId)
        {
            var result = await _service.GetMajorByTwoIdAsync(majorId, jobPositionId, new CancellationToken());
            return Ok(Result<List<MajorDto>>.Succeed(result));
        }

    }
}
