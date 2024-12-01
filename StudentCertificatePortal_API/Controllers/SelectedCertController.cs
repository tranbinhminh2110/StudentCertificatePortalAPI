using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class SelectedCertController : ApiControllerBase
    {
        private readonly ISelectedCertService _selectedCertService;

        public SelectedCertController(ISelectedCertService selectedCertService)
        {
            _selectedCertService = selectedCertService;
        }

        [HttpGet("user/{userId}/certs")]
        public async Task<IActionResult> GetCertsByUserId(int userId, CancellationToken cancellationToken)
        {
            try
            {
                var certifications = await _selectedCertService.GetCertsByUserId(userId, cancellationToken);

                if (certifications == null || certifications.Count == 0)
                {
                    return NotFound(new { message = "No certifications found for this user." });
                }
                var certSummary = certifications.Select(cert => new
                {
                    CertId = cert.CertId,
                    CertName = cert.CertName
                }).ToList();
                return Ok(certSummary);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("user/certs")]
        public async Task<IActionResult> SelectCertForUser([FromBody] CreateCertForUserRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.UserId != request.UserId)
                {
                    return BadRequest(new { message = "UserId in the URL does not match the UserId in the request body." });
                }

                bool result = await _selectedCertService.SelectedCertForUser(request, cancellationToken);

                if (result)
                {
                    return Ok(new { message = "Certifications added successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Failed to add certifications for the user." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPut("user/certs")]
        public async Task<IActionResult> UpdateCertForUser([FromBody] UpdateCertForUserRequest request, CancellationToken cancellationToken)
        {
            try
            {

                bool result = await _selectedCertService.UpdateCertForUser(request, cancellationToken);

                if (result)
                {
                    return Ok(new { message = "Certifications updated successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Failed to update certifications for the user." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpDelete("user/{userId}/certs/{certId}")]
        public async Task<IActionResult> DeleteCertForUser(int userId, int certId, CancellationToken cancellationToken)
        {
            try
            {
                bool result = await _selectedCertService.DeleteCertForUser(userId, certId, cancellationToken);

                if (result)
                {
                    return Ok(new { message = "Certification deleted successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Failed to delete certification for the user." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
}
}
