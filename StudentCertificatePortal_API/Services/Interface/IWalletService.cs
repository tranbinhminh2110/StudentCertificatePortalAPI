using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IWalletService
    {
        Task<WalletDto> CreateWalletAsync(int userId, CancellationToken cancellationToken);
        Task<List<WalletDto>> GetAll();
        Task<WalletDto> GetWalletByIdAsync(int userId, CancellationToken cancellationToken);
        Task<WalletDto> UpdateWalletAsync(int userId,int point , EnumWallet status, CancellationToken cancellationToken);
        Task<WalletDto> DeleteWalletAsync(int userId, CancellationToken cancellationToken);
        Task<WalletDto> GetWalletByWalletIdAsync(int walletId, CancellationToken cancellationToken);
    }
}
