using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ITopSearchService
    {
        Task<List<CertificationDto>> GetCertificationByTopSearchAsync(int topN);
       Task<List<SimulationExamDto>> GetSimulationExamByTopSearchAsync(int topN, Enums.EnumPermission permission);
    }
}
