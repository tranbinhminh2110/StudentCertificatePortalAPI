using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class PeerReviewController: ApiControllerBase
    {
        private readonly IPeerReviewService _peerReviewService;

        public PeerReviewController(IPeerReviewService peerReviewService)
        {
            _peerReviewService = peerReviewService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePeerReviewAsync([FromBody] CreatePeerReviewRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _peerReviewService.CreatePeerReviewAsync(request, cancellationToken);
                return Ok(Result<PeerReviewDto>.Succeed(result));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{peerReviewId}")]
        public async Task<IActionResult> DeletePeerReviewAsync(int peerReviewId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _peerReviewService.DeletePeerReviewAsync(peerReviewId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _peerReviewService.GetAll(cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{peerReviewId}")]
        public async Task<IActionResult> GetPeerReviewByIdAsync(int peerReviewId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _peerReviewService.GetPeerReviewByIdAsync(peerReviewId, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{peerReviewId}")]
        public async Task<IActionResult> UpdatePeerReviewAsync(int peerReviewId, [FromBody] UpdatePeerReviewRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _peerReviewService.UpdatePeerReviewAsync(peerReviewId, request, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("peer_review_for_reviewer/{examId}")]
        public async Task<ActionResult<List<PeerReviewForReviewerDto>>> GetListPeerReviewForExamAsync(int examId, CancellationToken cancellationToken)
        {
            try
            {
                var peerReviewDtos = await _peerReviewService.GetListPeerReviewAsyncForReviewer(examId, cancellationToken);

                if (peerReviewDtos == null || !peerReviewDtos.Any())
                {
                    return NotFound(new { Message = $"No peer reviews found for the exam with ID: {examId}" });
                }

                return Ok(peerReviewDtos); // Return 200 OK with the list of peer reviews
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
            }
        }
        [HttpGet("peer_review_for_reviewed/{scoreId}")]
        public async Task<ActionResult<List<PeerReviewForReviewerDto>>> GetListPeerReviewForReviewdUserAsync(int scoreId, CancellationToken cancellationToken)
        {
            try
            {
                var peerReviewDtos = await _peerReviewService.GetListPeerReviewAsyncForReviewedUser(scoreId, cancellationToken);

                if (peerReviewDtos == null || !peerReviewDtos.Any())
                {
                    return NotFound(new { Message = $"No peer reviews found for the score with ID: {scoreId}" });
                }

                return Ok(peerReviewDtos); // Return 200 OK with the list of peer reviews
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while processing your request.", Details = ex.Message });
            }
        }
    }
}
