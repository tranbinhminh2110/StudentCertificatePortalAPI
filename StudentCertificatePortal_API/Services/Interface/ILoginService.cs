using Microsoft.AspNetCore.Identity.Data;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ILoginService
    {
        Task<UserDto> Authenticate(LoginUserRequest loginRequest, CancellationToken cancellationToken);
    }
}
