using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ICheckoutService
    {
        public Task<CheckoutDto> CreatePaymentLinkAsync(int transId, CancellationToken cancellationToken);

        /*public Task<ResponseCheckoutData> GetInfoLinkPayment(int orderId, CancellationToken cancellationToken);*/
    }
}
