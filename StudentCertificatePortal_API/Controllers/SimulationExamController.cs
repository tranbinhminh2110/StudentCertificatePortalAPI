using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class SimulationExamController: ApiControllerBase
    {
        private readonly ISimulationExamService _service;
        private readonly IMapper _mapper;

        public SimulationExamController(ISimulationExamService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
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
    }
}
