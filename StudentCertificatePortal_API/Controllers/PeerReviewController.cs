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
    }
}
