using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Implemetation;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.Controllers
{
    public class SimulationExamController: ApiControllerBase
    {
        private readonly ISimulationExamService _service;
        private readonly IMapper _mapper;
        private readonly IPermissionService<SimulationExam> _simulationExamPermissionService;

        public SimulationExamController(ISimulationExamService service, IMapper mapper, 
            IPermissionService<SimulationExam> simulationExamPermissionService)
        {
            _service = service;
            _mapper = mapper;
            _simulationExamPermissionService = simulationExamPermissionService;
        }

        [HttpGet]
        public async Task<ActionResult<Result<List<SimulationExamDto>>>> GetAllExam()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<SimulationExamDto>>.Succeed(result));
        }
        [HttpGet("{examId:int}")]
        public async Task<ActionResult<Result<SimulationExamDto>>> GetSimulationExamById([FromRoute] int examId)
        {
            var result = await _service.GetSimulationExamByIdAsync(examId, new CancellationToken());
            return Ok(Result<SimulationExamDto>.Succeed(result));
        }
        [HttpGet("get-by-certId/{certId:int}")]
        public async Task<ActionResult<Result<List<SimulationExamDto>>>> GetSimulationExamByCertId([FromRoute] int certId)
        {
            var result = await _service.GetSimulationExamByCertIdAsync(certId, new CancellationToken());
            return Ok(Result<List<SimulationExamDto>>.Succeed(result));
        }

        [HttpPost]
        public async Task<ActionResult<Result<SimulationExamDto>>> CreateSimulationExam([FromBody] CreateSimulationExamRequest request)
        {
            var result = await _service.CreateSimulationExamAsync(request, new CancellationToken());
            return Ok(Result<SimulationExamDto>.Succeed(result));
        }
        [HttpPut("{examId:int}")]
        public async Task<ActionResult<Result<SimulationExamDto>>> UpdateSimulationExam(int examId, UpdateSimulationExamRequest request)
        {
            var result = await _service.UpdateSimulationExamAsync(examId, request, new CancellationToken());
            return Ok(Result<SimulationExamDto>.Succeed(result));
        }
        [HttpDelete("{examId:int}")]
        public async Task<ActionResult<Result<SimulationExamDto>>> DeleteSimulationExamById([FromRoute] int examId)
        {
            var result = await _service.DeleteSimulationExamAsync(examId, new CancellationToken());
            return Ok(Result<SimulationExamDto>.Succeed(result));
        }
        [HttpGet("~/api/v1/[controller]/search")]
        public async Task<ActionResult<Result<List<SimulationExamDto>>>> GetCourseByName([FromQuery] string? examName = null)
        {
            var result = await _service.GetSimulationExamByNameAsync(examName, new CancellationToken());
            return Ok(Result<List<SimulationExamDto>>.Succeed(result));
        }
        [HttpPut("update-permission/{examId:int}")]
        public async Task<ActionResult<Result<CertificationDto>>> UpdatePermissionSimulationExam(int examId, Enums.EnumPermission permission)
        {
            var result = await _simulationExamPermissionService.UpdatePermissionAsync(examId, permission, new CancellationToken());
            return result ? Ok("The Simulation Exam permission has been updated successfully.") : NotFound("The Simulation Exam with the specified ID was not found or the permission update failed.");
        }
        [HttpPut("{examId}/update-vouchers")]
        public async Task<IActionResult> UpdateExamVouchers(int examId, [FromBody] List<int> voucherIds)
        {
            var result = await _service.UpdateExamVouchersAsync(examId, voucherIds, HttpContext.RequestAborted);
            return Ok(result);  
        }
    }
}
