using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IPaymentService
    {
        Task<List<PaymentDto>> GetAll();
        Task<PaymentDto> GetPaymentByIdAsync(int paymentId);
        Task<PaymentDto> GetPaymentByUserIdAsync(int userId);
        Task<PaymentDto> ProcessPayment(CreatePaymentRequest request, CancellationToken cancellation);
    }
}
