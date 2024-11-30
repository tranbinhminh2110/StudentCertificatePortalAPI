using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
using System.Threading;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class RefundService : IRefundService
    {
        private readonly IUnitOfWork _uow;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _hubContext;
        public readonly IEmailService _emailService;
        public RefundService(IUnitOfWork uow, INotificationService notificationService, IHubContext<NotificationHub> hubContext, IEmailService emailService)
        {
            _uow = uow;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _emailService = emailService;
        }

        public async Task<bool> ProcessRefund(RefundRequest request, CancellationToken cancellationToken)
        {
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.WalletId == request.WalletId, cancellationToken, include: w => w.Include(c => c.User));
            if (wallet == null) { throw new KeyNotFoundException("Wallet not found!"); }

            if (request.Point > wallet.Point)
            {
                return false;
            }

            try
            {
                wallet.Point -= request.Point;
                _uow.WalletRepository.Update(wallet);
                var transaction = new Transaction()
                {
                    TransDesription = $"Transaction for Wallet ID {request.WalletId} with {request.Point} points has been refunded. The new balance is {wallet.Point} points.",
                    Amount = request.Point * 1000,
                    TransStatus = Enums.EnumTransaction.Refunded.ToString(),
                    WalletId = request.WalletId,
                    Point = -request.Point,
                };
                await _uow.TransactionRepository.AddAsync(transaction);
                await _uow.Commit(cancellationToken);

                var emailSubject = "Refund Processed Successfully";
                var emailBody = $"Dear {wallet.User.Fullname},\n\n" +
                                $"Your refund request of {request.Point} points has been processed successfully. " +
                                $"Your current wallet balance is {wallet.Point} points.\n\nThank you.";
                await _emailService.SendEmailAsync(wallet.User.Email, emailSubject, emailBody);


                return true;
            }
            catch(Exception ex)
            {
                return false;
            }

        }

        public async Task<bool> SendRequestRefund(RefundRequest request, CancellationToken cancellationToken)
        {
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.WalletId == request.WalletId, cancellationToken, include: w => w.Include(u => u.User)) ;
            if(wallet == null) { throw new KeyNotFoundException("Wallet not found!"); }
            
            if (request.Point > wallet.Point)
            {
                return false;
            }
            var notification = new Notification
            {
                NotificationName = "Refund Request",
                NotificationDescription = $"A refund request for Wallet ID {request.WalletId} (Name: {wallet.User.Fullname} with {request.Point} points has been created and is pending approval.",
                CreationDate = DateTime.UtcNow,
                Role = "Admin",
                IsRead = false,
            };

            await _uow.NotificationRepository.AddAsync(notification);
            await _uow.Commit(cancellationToken);
            var notifications = await _notificationService.GetNotificationByRoleAsync("Admin", new CancellationToken());
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);

            return true;
        }

        
    }
}
