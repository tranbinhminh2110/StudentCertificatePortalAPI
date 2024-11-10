using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.DTOs;
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
    }
}
