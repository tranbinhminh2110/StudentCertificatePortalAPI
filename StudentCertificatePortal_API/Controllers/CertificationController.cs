﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.Controllers
{
    public class CertificationController: ApiControllerBase
    {
        private readonly ICertificationService _service;
        private readonly IMapper _mapper;
        private readonly IPermissionService<Certification> _certificationPermissionService;

        public CertificationController(ICertificationService service, IMapper mapper,
            IPermissionService<Certification> certificationPermissionService)
        {
            _service = service;
            _mapper = mapper;
            _certificationPermissionService = certificationPermissionService;
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

        [HttpGet("~/api/v1/[controller]/search")]
        public async Task<ActionResult<Result<List<CertificationDto>>>> GetCertificcationByName([FromQuery] string? certName = null)
        {
            IEnumerable<CertificationDto> result;
            if(certName == null)
            {
                result = await _service.GetAll();
            }
            else
            {
                result = await _service.GetCertificationByNameAsync(certName, new CancellationToken());
            }
            
            return Ok(Result<List<CertificationDto>>.Succeed(result.ToList()));
        }
        [HttpDelete("{certId:int}")]
        public async Task<ActionResult<Result<CertificationDto>>> DeleteCertificationById([FromRoute] int certId)
        {
            var result = await _service.DeleteCertificationAsync(certId, new CancellationToken());
            return Ok(Result<CertificationDto>.Succeed(result));
        }

        [HttpPut("update-permission/{certId:int}")]
        public async Task<ActionResult<Result<CertificationDto>>> UpdatePermissionCertification(int certId, Enums.EnumPermission permission)
        {
            var result = await _certificationPermissionService.UpdatePermissionAsync(certId, permission, new CancellationToken());
            return result ? Ok("The certification permission has been updated successfully.") : NotFound("The certification with the specified ID was not found or the permission update failed.");
        }

    }
}
