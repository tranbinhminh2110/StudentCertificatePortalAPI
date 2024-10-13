using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class CheckoutService : ICheckoutService
    {
        public Task<CheckoutDto> CreatePaymentLinkAsync(int orderId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
