using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IUserService
    {
        Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken);
        Task<List<UserDto>> GetAll();

        Task<UserDto> CreateRegisterUserAsync(CreateRegisterUserRequest request, CancellationToken cancellationToken);
        Task<UserDto> GetUserByIdAsync(int userId, CancellationToken cancellationToken);
        Task<UserDto> UpdateUserAsync(int userId, UpdateUserRequest request, CancellationToken cancellationToken);
        Task<UserDto> DeleteUserByIdAsync(int userId, CancellationToken cancellationToken);
    }
}
