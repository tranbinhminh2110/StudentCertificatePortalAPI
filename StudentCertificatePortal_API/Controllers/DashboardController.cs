using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Services.Implemetation;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class DashboardController : ApiControllerBase
    {
        private readonly IDashboardService _service;
        public DashboardController(IDashboardService service)
        {
            _service = service;
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummaryDto>> GetDashboardSummary()
        {
            var summary = await _service.GetDashboardSummaryAsync(new CancellationToken());
            return Ok(Result<DashboardSummaryDto>.Succeed(summary));
        }

        [HttpGet("total-point")]
        public async Task<IActionResult> GetTotalAmount(
            [FromQuery] TimePeriod period,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                decimal totalPoint = await _service.GetTotalAmountAsync(period, startDate, endDate);
                return Ok(Result<decimal>.Succeed(totalPoint));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred.", details = ex.Message });
            }
        }
}}
