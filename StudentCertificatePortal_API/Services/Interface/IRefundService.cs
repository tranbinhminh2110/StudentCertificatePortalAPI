using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IRefundService
    {
        Task<bool> SendRequestRefund(RefundRequest request, CancellationToken cancellationToken);
        Task<bool> ProcessRefund (RefundRequest request, CancellationToken cancellationToken);  
    }
}
