using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.Contracts.Responses;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
using System.Net.Http.Headers;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;


        public WalletService(IUnitOfWork uow, IMapper mapper, IConfiguration configuration)
        {
            _uow = uow;
            _mapper = mapper;
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api-merchant.payos.vn/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("x-client-id", _configuration["PayOS:ClientId"]);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _configuration["PayOS:APIKey"]);
        }
        public async Task<WalletDto> CreateWalletAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == userId);

            
            if (wallet != null)
            {
                throw new Exception("Wallet is existed");
            }

            var newWallet = new Wallet()
            {
                UserId = userId,
                Point = 0,
                WalletStatus = EnumWallet.IsUsed.ToString(),
            };

             var result =await _uow.WalletRepository.AddAsync(newWallet);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<WalletDto>(result);

        }

        public async Task<WalletDto> DeleteWalletAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == userId,
                cancellationToken);

            if(wallet == null || wallet.WalletStatus == EnumWallet.IsLocked.ToString())
            {
                throw new KeyNotFoundException("Wallet not found or wallet is locked.");
            }

            wallet.WalletStatus = EnumWallet.IsLocked.ToString();

            var result = _uow.WalletRepository.Update(wallet);

            return _mapper.Map<WalletDto>(result);
        }

        public async Task<List<WalletDto>> GetAll()
        {
            var wallets = await _uow.WalletRepository.GetAll();

            return _mapper.Map<List<WalletDto>>(wallets);
        }

        public async Task<WalletDto> GetWalletByIdAsync(int userId, int transactionId, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found.");
                }

                var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == userId);
                if (wallet == null || wallet.WalletStatus == EnumWallet.IsLocked.ToString())
                {
                    throw new Exception("Wallet of user not found or is locked. Please contact support.");
                }

                var transaction = await _uow.TransactionRepository.FirstOrDefaultAsync(x => x.TransactionId == transactionId);
                if (transaction != null && transaction.TransStatus == Enums.EnumTransaction.Pending.ToString())
                {
                    if (transactionId > 0)
                    {
                        var response = await _httpClient.GetAsync($"v2/payment-requests/{transactionId}");
                        if (!response.IsSuccessStatusCode)
                        {
                            throw new KeyNotFoundException($"Transaction Id {transactionId} not found.");
                        }

                        var responseContent = await response.Content.ReadAsStringAsync();
                        var paymentResponse = JsonConvert.DeserializeObject<PaymentRequestResponse>(responseContent);

                        if (paymentResponse?.Data?.Status == "PAID")
                        {
                            transaction.TransStatus = Enums.EnumTransaction.Success.ToString();
                            transaction.TransDesription = string.Empty;

                            foreach (var datapmt in paymentResponse.Data.Transactions)
                            {
                                transaction.TransDesription += $"Payment for Order #{paymentResponse.Data.OrderCode} - Account: {datapmt.AccountNumber} on {datapmt.TransactionDateTime:yyyy-MM-dd HH:mm}.";
                            }

                            wallet.Point += transaction.Point;
                            _uow.WalletRepository.Update(wallet);
                            _uow.TransactionRepository.Update(transaction);
                            await _uow.Commit(cancellationToken);
                        }
                        else
                        {
                            throw new Exception("Transaction not marked as PAID.");
                        }
                    }
                    else if (transactionId == 0)
                    {
                        wallet.Point += transaction.Point;
                        transaction.TransStatus = Enums.EnumTransaction.Canceled.ToString();
                        _uow.WalletRepository.Update(wallet);
                        _uow.TransactionRepository.Update(transaction);
                        await _uow.Commit(cancellationToken);
                    }
                }

                return _mapper.Map<WalletDto>(wallet);
            }
            catch (DbUpdateException upe)
            {
                var innerMessage = upe.InnerException?.Message ?? "Database update error.";
                throw new Exception($"An error occurred while updating the wallet or transaction: {innerMessage}", upe);
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? "No inner exception details available.";
                throw new Exception($"An unexpected error occurred: {innerMessage}", ex);
            }
        }


        public async Task<WalletDto> GetWalletByWalletIdAsync(int walletId, CancellationToken cancellationToken)
        {
            
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.WalletId == walletId);

            if (wallet == null || wallet.WalletStatus == EnumWallet.IsLocked.ToString())
            {
                throw new Exception("Wallet of user not found or is locked. Contact with me to support.");
            }

            return _mapper.Map<WalletDto>(wallet);
        }

        public async Task<WalletDto> UpdateWalletAsync(int userId, int point, EnumWallet status, CancellationToken cancellationToken)
        {
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

            if(wallet == null)
            {
                throw new KeyNotFoundException("Wallet not found.");
            }

            wallet.Point += point;
            wallet.WalletStatus = status == EnumWallet.None ? wallet.WalletStatus : status.ToString();

            var result = _uow.WalletRepository.Update(wallet);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<WalletDto>(result);
        }
    }
}
