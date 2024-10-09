using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class ExamSessionController : ApiControllerBase
    {
        private readonly IExamSessionService _service;
        private readonly IMapper _mapper;
        public ExamSessionController(IExamSessionService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<Result<List<ExamSessionDto>>>> GetAllExamSession()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<ExamSessionDto>>.Succeed(result));
        }
        [HttpGet("{sessionId:int}")]
        public async Task<ActionResult<Result<ExamSessionDto>>> GetExamSessionById([FromRoute] int sessionId)
        {
            var result = await _service.GetExamSessionByIdAsync(sessionId, new CancellationToken());
            return Ok(Result<ExamSessionDto>.Succeed(result));
        }
        [HttpPost]
        public async Task<ActionResult<Result<ExamSessionDto>>> CreateExamSession([FromBody] CreateExamSessionRequest request)
        {
            var result = await _service.CreateExamSessionAsync(request, new CancellationToken());
            return Ok(Result<ExamSessionDto>.Succeed(result));
        }
        [HttpPut("{sessionId:int}")]
        public async Task<ActionResult<Result<ExamSessionDto>>> UpdateExamSession(int sessionId, UpdateExamSessionRequest request)
        {
            var result = await _service.UpdateExamSessionAsync(sessionId, request, new CancellationToken());
            return Ok(Result<ExamSessionDto>.Succeed(result));
        }
        [HttpDelete("{sessionId:int}")]
        public async Task<ActionResult<Result<ExamSessionDto>>> DeleteExamSessionById([FromRoute] int sessionId)
        {
            var result = await _service.DeleteExamSessionAsync(sessionId, new CancellationToken());
            return Ok(Result<ExamSessionDto>.Succeed(result));
        }
        [HttpGet("{sessionName}")]
        public async Task<ActionResult<Result<List<ExamSessionDto>>>> GetExamSessionByName([FromRoute] string sessionName)
        {
            var result = await _service.GetExamSessionByNameAsync(sessionName, new CancellationToken());
            return Ok(Result<List<ExamSessionDto>>.Succeed(result));
        }

    }
}
