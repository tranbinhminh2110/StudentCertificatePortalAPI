using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class FeedbackController : ApiControllerBase
    {
        private readonly IFeedbackService _service;
        private readonly IUnitOfWork _uow;

        public FeedbackController(IFeedbackService service, IUnitOfWork uow)
        {
            _service = service;
            _uow = uow;
        }
        [HttpGet]
        public async Task<ActionResult<Result<List<FeedbackDto>>>> GetallFeedback()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<FeedbackDto>>.Succeed(result));
        }
        [HttpGet("{feedbackId:int}")]
        public async Task<ActionResult<Result<FeedbackDto>>> GetFeedbackById([FromRoute] int feedbackId)
        {
            var result = await _service.GetFeedbackByIdAsync(feedbackId, new CancellationToken());
            return Ok(Result<FeedbackDto>.Succeed(result));
        }
        [HttpPost]
        public async Task<ActionResult<Result<FeedbackDto>>> CreateFeedback([FromBody] CreateFeedbackRequest request)
        {
            var result = await _service.CreateFeedbackAsync(request, new CancellationToken());
            return Ok(Result<FeedbackDto>.Succeed(result));
        }
        [HttpPut("{feedbackId:int}")]
        public async Task<ActionResult<Result<FeedbackDto>>> UpdateFeedback(int feedbackId, UpdateFeedbackRequest request)
        {
            var result = await _service.UpdateFeedbackAsync(feedbackId, request, new CancellationToken());
            return Ok(Result<FeedbackDto>.Succeed(result));
        }
        [HttpDelete("{feedbackId:int}")]
        public async Task<ActionResult<Result<FeedbackDto>>> DeleteFeedbackById([FromRoute] int feedbackId)
        {
            var result = await _service.DeleteFeedbackAsync(feedbackId, new CancellationToken());
            return Ok(Result<FeedbackDto>.Succeed(result));
        }
        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<Result<List<FeedbackDto>>>> GetFeedbackByUserId([FromRoute] int userId)
        {
            var result = await _service.GetFeedbackByUserIdAsync(userId, new CancellationToken());
            return Ok(Result<List<FeedbackDto>>.Succeed(result));
        }
        [HttpGet("exam/{examId:int}")]
        public async Task<ActionResult<Result<List<FeedbackDto>>>> GetFeedbackByExamId([FromRoute] int examId)
        {
            var result = await _service.GetFeedbackByExamIdAsync(examId, new CancellationToken());
            return Ok(Result<List<FeedbackDto>>.Succeed(result));
        }
        [HttpGet("cert/{certId:int}")]
        public async Task<ActionResult<Result<List<FeedbackDto>>>> GetFeedbackByCertId([FromRoute] int certId)
        {
            var result = await _service.GetFeedbackByCertIdAsync(certId, new CancellationToken());
            return Ok(Result<List<FeedbackDto>>.Succeed(result));
        }
        
    }
}
