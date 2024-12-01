using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ISelectedCertService
    {
        Task<bool> UpdateCertForUser(UpdateCertForUserRequest request, CancellationToken cancellationToken);
        Task<bool> SelectedCertForUser(CreateCertForUserRequest request, CancellationToken cancellationToken);
        Task<bool> DeleteCertForUser(int userId, int certId, CancellationToken cancellationToken);
        Task<List<CertificationDto>> GetCertsByUserId(int userId, CancellationToken cancellationToken);
    }
}
