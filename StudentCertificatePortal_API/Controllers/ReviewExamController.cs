using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Implemetation;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class ReviewExamController: ApiControllerBase
    {

        private readonly IReviewExamService _service;

        public ReviewExamController(IReviewExamService service)
        {
            _service = service;
        }

        [HttpGet("review")]
        public async Task<ActionResult<ExamReviewDto>> GetExamReviewAsync(int examId, int userId, int scoreId, CancellationToken cancellationToken)
        {
            try
            {
                var examReview = await _service.GetExamReviewAsync(examId, userId, scoreId, cancellationToken);
                return Ok(examReview); // Return the exam review as HTTP 200 OK
            }
            catch (KeyNotFoundException ex)
            {
                // Handle not found errors (e.g. no answers found, score record not found)
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                // Handle other invalid operations (e.g. user not enrolled)
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Generic error handling
                return StatusCode(500, new { Message = "An unexpected error occurred.", Detail = ex.Message });
            }
        }
    }
}
