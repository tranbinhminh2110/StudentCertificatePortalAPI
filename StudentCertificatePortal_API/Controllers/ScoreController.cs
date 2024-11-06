using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Contracts.Requests;
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
    }
}
