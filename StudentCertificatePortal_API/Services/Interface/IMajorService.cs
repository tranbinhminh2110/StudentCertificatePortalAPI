using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IMajorService
    {
        Task<MajorDto> CreateMajorAsync(CreateMajorRequest request, CancellationToken cancellationToken);
        Task<List<MajorDto>> GetAll();
        Task<MajorDto> GetMajorByIdAsync(int majorId, CancellationToken cancellationToken);
        Task<MajorDto> UpdateMajorAsync(int majorId, UpdateMajorRequest request, CancellationToken cancellationToken);
        Task<MajorDto> DeleteMajorAsync(int majorId, CancellationToken cancellationToken);
    }
}
