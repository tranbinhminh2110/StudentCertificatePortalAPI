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
        [HttpGet("monthly-revenue/{year}")]
        public async Task<IActionResult> GetMonthlyRevenue(int year, CancellationToken cancellationToken)
        {
            try
            {
                Dictionary<int, decimal> monthlyRevenue = await _service.GetMonthlyRevenueAsync(year, cancellationToken);

                return Ok(Result<Dictionary<int, decimal>>.Succeed(monthlyRevenue));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while calculating monthly revenue." + ex.Message);
            }
        }

        [HttpGet("weekly-revenue/{year}/{month}")]
        public async Task<IActionResult> GetWeeklyRevenue(int year, int month, CancellationToken cancellationToken)
        {
            try
            {
                
                Dictionary<int, decimal> weeklyRevenue = await _service.GetWeeklyRevenueAsync(year, month, cancellationToken);

                return Ok(Result<Dictionary<int, decimal>>.Succeed(weeklyRevenue));
            }
            catch (Exception ex)
            {

                return StatusCode(500, "An error occurred while calculating weekly revenue." + ex.Message);
            }
        }
    }
}
