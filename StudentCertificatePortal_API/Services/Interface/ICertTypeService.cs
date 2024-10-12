using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ICertTypeService
    {
        Task<CertTypeDto> CreateCertTypeAsync(CreateCertTypeRequest request, CancellationToken cancellationToken);
        Task<List<CertTypeDto>> GetAll();
        Task<CertTypeDto> GetCertTypeByIdAsync(int certTypeId, CancellationToken cancellationToken);
        Task<CertTypeDto> UpdateCertTypeAsync(int certTypeId, UpdateCertTypeRequest request, CancellationToken cancellationToken);
        Task<CertTypeDto> DeleteCertTypeAsync(int certTypeId, CancellationToken cancellationToken);
        Task<List<CertTypeDto>> GetCertTypeByNameAsync(string certTypeName, CancellationToken cancellationToken);
    }
}
