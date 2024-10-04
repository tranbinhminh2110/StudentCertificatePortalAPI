using Microsoft.AspNetCore.Identity;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IProfileService
    {
        Task<UserDto> FindByEmailAsync(string email);
        Task<IdentityResult> ResetPasswordAsync(string email, string newPassword);
        Task<UserDto> GetProfileByIdAsync(int userId, CancellationToken cancellationToken);
        Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken);
    }
}
