using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Controllers
{
    [ApiController]
    [Route("~/api/v1/[controller]")]
    public class ApiControllerBase : ControllerBase
    {
        [HttpDelete("{certId:int}")]
        public async Task<ActionResult<Result<CertificationDto>>> DeleteCertificationById([FromRoute] int certId)
        {
            var result = await _service.DeleteCertificationAsync(certId, new CancellationToken());
            return Ok(Result<CertificationDto>.Succeed(result));
        }
    }
}
