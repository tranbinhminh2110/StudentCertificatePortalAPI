using AutoMapper;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class CheckoutService : ICheckoutService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CheckoutService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<CheckoutDto> CreatePaymentLinkAsync(int transId, CancellationToken cancellationToken)
        {
            var transaction = await _uow.TransactionRepository.FirstOrDefaultAsync(x => x.TransactionId == transId);


            if (transaction == null)
            {
                throw new KeyNotFoundException("Transaction not found.");
            }

            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.WalletId == transaction.WalletId);

            if(wallet == null || wallet.WalletStatus == EnumWallet.IsLocked.ToString())
            {
                throw new KeyNotFoundException("Wallet not found or is locked.");
            }

            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == wallet.UserId);

            if(user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var currentTime = DateTimeOffset.UtcNow;
            var expirationTime = currentTime.AddHours(1);

            var data = new CheckoutDto()
            {
                OrderCode = transId,
                Amount = transaction.Amount,
                Description = "UniCert Payment",
                BuyerName = user.Fullname?.Trim() ?? string.Empty,
                BuyerEmail = user.Email?.Trim() ?? string.Empty,
                BuyerPhone = user.PhoneNumber?.Trim() ?? string.Empty,
                BuyerAddress = user.Address?.Trim() ?? string.Empty,
                CancelUrl = "https://artspectrum.vercel.app/cart",
                ReturnUrl = "https://artspectrum.vercel.app/checkout-success",
                ExpiredAt = (int)expirationTime.ToUnixTimeSeconds(), // Set the expiration time to be in the future
                Signature = "string",
                Items = new List<WalletRequest>()
            };

            data.Items.Add(new WalletRequest()
            {
                Title = "Recharge Points Directly",
                Point = transaction.Point,
                Price = transaction.Amount
            });

            return data;

        }
    }
}
