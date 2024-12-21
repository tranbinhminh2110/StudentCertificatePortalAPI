using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class TopSearchController : ApiControllerBase
    {
        private readonly ITopSearchService _service;

        public TopSearchController(ITopSearchService service)
        {
            _service = service;
        }
        [HttpGet("{topN:int}")]
        public async Task<IActionResult> TopSearchCertification([FromRoute] int topN)
        {
            var result = await _service.GetCertificationByTopSearchAsync(topN);
            return Ok(Result<List<CertificationDto>>.Succeed(result));
        }

        [HttpGet("simulation-exams/{topN:int}/{permission}")]
        public async Task<IActionResult> TopSearchSimulationExam([FromRoute] int topN, [FromRoute] EnumPermission permission)
        {
            if (topN <= 0)
            {
                return BadRequest("The number of exams must be greater than zero.");
            }

            try
            {
                var result = await _service.GetSimulationExamByTopSearchAsync(topN, permission);
                return Ok(Result<List<SimulationExamDto>>.Succeed(result));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

    }
}
