using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.Services.Implemetation;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class RefundController: ApiControllerBase
    {
        private readonly IRefundService _service;
        public RefundController(IRefundService service) { _service = service; }

        [HttpPost]
        public async Task<IActionResult> RequestRefund ([FromBody] RefundRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _service.SendRequestRefund(request, CancellationToken.None);
                if (result)
                {
                    return Ok(new { Message = "Refund request has been submitted successfully and is pending approval." });
                }

                return BadRequest(new { Message = "Refund request failed. Either the wallet does not exist or points are insufficient." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred.",
                    Details = ex.Message
                });
            }
        }
        [HttpPost("ProcessRefund")]
        public async Task<IActionResult> ProcessRefund([FromBody] RefundRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.ProcessRefund(request, cancellationToken);

                if (result)
                {
                    return Ok(new { Message = "Refund processed successfully." });
                }

                return BadRequest(new { Message = "Insufficient points in the wallet." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "An unexpected error occurred while processing the refund.",
                    Details = ex.Message
                });
            }
        }
    }
}
