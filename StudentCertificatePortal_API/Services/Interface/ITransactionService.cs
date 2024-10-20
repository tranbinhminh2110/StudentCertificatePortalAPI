using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ITransactionService
    {
        Task<TransactionDto> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken);
        Task<List<TransactionDto>> GetAll();
        Task<TransactionDto> GetTransactionByIdAsync(int transId, CancellationToken cancellationToken);
        Task<TransactionDto> UpdateTransactionAsync(int transId, UpdateTransactionRequest request, CancellationToken cancellationToken);
        Task<TransactionDto> UpdateStatusTransactionAsync(int transId, Enums.EnumTransaction status, CancellationToken cancellationToken);
        Task<TransactionDto> DeleteTransactionAsync(int transId, CancellationToken cancellationToken);
        Task<List<TransactionDto>> GetTransactionByNameAsync(string transName, CancellationToken cancellationToken);

        Task<List<TransactionDto>> GetTransactionByUserId (int userId , CancellationToken cancellationToken);
    }
}
