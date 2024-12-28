using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IVoucherService
    {
        Task<VoucherDto> CreateVoucherAsync(CreateVoucherRequest request, CancellationToken cancellationToken);
        Task<List<VoucherDto>> GetAll(CancellationToken cancellationToken);
        Task<VoucherDto> GetVoucherByIdAsync(int voucherId, CancellationToken cancellationToken);
        Task<VoucherDto> UpdateVoucherAsync(int voucherId, UpdateVoucherRequest request, CancellationToken cancellationToken);
        Task<VoucherDto> DeleteVoucherAsync(int voucherId, CancellationToken cancellationToken);
        Task<List<VoucherDto>> GetVoucherByNameAsync(string voucherName, CancellationToken cancellationToken);
        Task<List<VoucherDto>> GetVouchersByUserLevelAsync(int userId, CancellationToken cancellationToken);
    }
}
