using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ICartService
    {
        Task<CartDto> CreateCartAsync(CreateCartRequest request, CancellationToken cancellationToken);
        Task<List<CartDto>> GetAll(CancellationToken cancellationToken);
        Task<CartDto> GetCartByIdAsync(int cartId, CancellationToken cancellationToken);
        Task<CartDto> UpdateCartAsync(int cartId, UpdateCartRequest request, CancellationToken cancellationToken);
        Task<CartDto> DeleteCartAsync(int cartId, CancellationToken cancellationToken);
        Task<List<CartDto>> CreateCartsForAllUsersWithoutCartsAsync(CancellationToken cancellationToken);
    }
}
