using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
using System.Text;
using System.Text.Json;
using System.Threading;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class RefundService : IRefundService
    {
        private readonly IUnitOfWork _uow;
        public readonly IEmailService _emailService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCacheService _cacheService;
        public RefundService(IUnitOfWork uow, IEmailService emailService, IHttpClientFactory httpClientFactory, IMemoryCacheService cacheService)
        {
            _uow = uow;
            _emailService = emailService;
            _httpClientFactory = httpClientFactory;
            _cacheService = cacheService;
        }

        public async Task<bool> ProcessRefund(ProcessRefundRequest request, CancellationToken cancellationToken)
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
                    TransDesription = $"A refund of {request.Point} points has been processed. The new balance is {wallet.Point} points.",
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
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.WalletId == request.WalletId, cancellationToken, include: w => w.Include(u => u.User));
            if (wallet == null)
            {
                throw new KeyNotFoundException("Wallet not found!");
            }


            if (request.Point > wallet.Point)
            {
                return false; 
            }

            var cacheKey = $"refund_requests_{request.WalletId}";
            var currentRequests = await _cacheService.GetAsync<int>(cacheKey);

            int maxRequestsPerDay = 2; 

            if (currentRequests >= maxRequestsPerDay)
            {
                throw new Exception($"User {wallet.User.Fullname} has exceeded the request limit of {maxRequestsPerDay} refund requests in the last 24 hours.");
            }

            await _cacheService.SetAsync(cacheKey, currentRequests + 1, TimeSpan.FromHours(24)); 

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync("https://api.httzip.com/api/bank/list");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to fetch banks data. Status code: {(int)response.StatusCode}");
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var banksResponse = JsonSerializer.Deserialize<ApiResponse<BankDto>>(responseData, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var banks = banksResponse?.Data ?? new List<BankDto>();
                var bank = banks.FirstOrDefault(b => b.Code == request.BankAccount.BankCode);
                if (bank == null)
                {
                    throw new Exception("Bank with the provided code not found.");
                }

                string bankName = bank.Name;

                // Gửi email thông báo cho admin
                string adminEmail = "unicert79@gmail.com";
                var emailSubject = "Refund Request Created";
                var emailBody = new StringBuilder();
                emailBody.AppendLine("Dear Admin,");
                emailBody.AppendLine();
                emailBody.AppendLine($"A refund request has been created for the user {wallet.User.Fullname}.");
                emailBody.AppendLine();
                emailBody.AppendLine("Refund Details:");
                emailBody.AppendLine($"- User Name: {wallet.User.Fullname}");
                emailBody.AppendLine($"- Bank Name: {bankName}");
                emailBody.AppendLine($"- Account Number: {request.BankAccount.AccountNumber}");
                emailBody.AppendLine($"- Points Requested: {request.Point}");
                emailBody.AppendLine($"- Wallet Balance: {wallet.Point}");
                emailBody.AppendLine();
                emailBody.AppendLine("Please process this request as soon as possible.");
                emailBody.AppendLine();
                emailBody.AppendLine("Thank you.");

                await _emailService.SendEmailAsync(adminEmail, emailSubject, emailBody.ToString());
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }



    }
}
