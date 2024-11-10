using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ITopSearchService
    {
        Task<List<CertificationDto>> GetCertificationByTopSearchAsync(int topN);
    }
}
