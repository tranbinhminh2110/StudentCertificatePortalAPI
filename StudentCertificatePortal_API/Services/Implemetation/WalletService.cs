using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;


        public WalletService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
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

        public async Task<WalletDto> GetWalletByIdAsync(int userId, CancellationToken cancellationToken)
        {
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == userId);

            if(wallet == null)
            {
                throw new Exception("Wallet of user not found.");
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
