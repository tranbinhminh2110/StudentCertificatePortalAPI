using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Services.Implemetation;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class OrganizeController: ApiControllerBase
    {
        private readonly IOrganizeService _service;
        private readonly IMapper _mapper;

        public OrganizeController(IOrganizeService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<Result<List<OrganizeDto>>>> GetAllOrganize()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<OrganizeDto>>.Succeed(result));
        }
        [HttpGet("{organizeId:int}")]
        public async Task<ActionResult<Result<OrganizeDto>>> GetOrganizeById([FromRoute] int organizeId)
        {
            var result = await _service.GetOrganizeByIdAsync(organizeId, new CancellationToken());
            return Ok(Result<OrganizeDto>.Succeed(result));
        }
        [HttpPost]
        public async Task<ActionResult<Result<OrganizeDto>>> CreateOrganize([FromBody] CreateOrganizeRequest request)
        {
            var result = await _service.CreateOrganizeAsync(request,new CancellationToken());
            return Ok(Result<OrganizeDto>.Succeed(result));
        }
        [HttpPut("{organizeId:int}")]
        public async Task<ActionResult<Result<OrganizeDto>>> UpdateOrganize(int organizeId, UpdateOrganizeRequest request)
        {
            var result = await _service.UpdateOrganizeAsync(organizeId, request, new CancellationToken());
            return Ok(Result<OrganizeDto>.Succeed(result));
        }
        [HttpDelete("{organizeId:int}")]
        public async Task<ActionResult<Result<OrganizeDto>>> DeleteOrganizeById([FromRoute] int organizeId)
        {
            var result = await _service.DeleteOrganizeAsync(organizeId, new CancellationToken());
            return Ok(Result<OrganizeDto>.Succeed(result));
        }
        [HttpGet("{organizeName}")]
        public async Task<ActionResult<Result<List<OrganizeDto>>>> GetOrganizeByName([FromRoute] string organizeName)
        {
            var result = await _service.GetOrganizeByNameAsync(organizeName, new CancellationToken());
            return Ok(Result<List<OrganizeDto>>.Succeed(result));
        }
        [HttpPut("Permission")]
        public async Task<ActionResult<Result<OrganizeDto>>> UpdateOrganizePermission(int organizeId, [FromQuery] EnumPermission organizePermission)
        {
            var result = await _service.UpdateOrganizePermissionAsync(organizeId, organizePermission, new CancellationToken());
            return Ok(Result<OrganizeDto>.Succeed(result));
        }
    }
}
