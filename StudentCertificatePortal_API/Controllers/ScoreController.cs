using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class ScoreController : ApiControllerBase
    {
        private readonly IScoreService _service;
        public ScoreController(IScoreService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Scoring(UserAnswerRequest request, CancellationToken cancellationToken)
        {
            var result = await _service.Scoring(request, cancellationToken);
            return Ok(result);
        }

        [HttpGet("{userId:int}")]
        public async Task<IActionResult> GetScoreByUserId([FromRoute] int userId, [FromQuery] int? examId, CancellationToken cancellationToken)
        {
            var result = await _service.GetScoreByUserId(userId, examId, cancellationToken);
            return Ok(Result<List<ScoreDto>>.Succeed(result));
        }
    }
}
