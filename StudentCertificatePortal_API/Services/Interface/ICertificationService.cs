using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ICertificationService
    {
        Task<CertificationDto> CreateCertificationAsync(CreateCertificationRequest request, CancellationToken cancellationToken);
        Task<List<CertificationDto>> GetAll();
        Task<CertificationDto> GetCertificationById(int certificationId, CancellationToken cancellationToken);
        Task<CertificationDto> UpdateCertificationAsync(int certificationId, UpdateCertificationRequest request, CancellationToken cancellationToken);
        Task<CertificationDto> DeleteCertificationAsync(int certificationId, CancellationToken cancellationToken);

        Task<List<CertificationDto>> GetCertificationByNameAsync(string certName, CancellationToken cancellationToken);
    }
}
