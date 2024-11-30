using AutoMapper;
using FluentValidation;
using StackExchange.Redis;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateTransactionRequest> _addTransactionValidator;
        private readonly IValidator<UpdateTransactionRequest> _updateTransactionValidator;
        public TransactionService(IUnitOfWork uow, IMapper mapper,
            IValidator<CreateTransactionRequest> addTransactionValidator,
            IValidator<UpdateTransactionRequest> updateTransactionValidator) {
            
            _uow = uow;
            _mapper = mapper;
            _addTransactionValidator = addTransactionValidator;
            _updateTransactionValidator = updateTransactionValidator;
        }


        public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addTransactionValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.WalletId == request.WalletId);

            if(wallet== null || wallet.WalletStatus == EnumWallet.IsLocked.ToString())
            {
                throw new KeyNotFoundException("Wallet not created or is locked.");
            }
            var transactionEntity = new Transaction()
            {
                TransStatus = EnumTransaction.Pending.ToString(),
                WalletId = request.WalletId,
                Point = request.Point,
                Amount = request.Point*1000
            };
            var result = await _uow.TransactionRepository.AddAsync(transactionEntity);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<TransactionDto>(result);
        }

        /*public async Task<TransactionDto> CreateTransactionRefundAsync(CreateTransactionRequest request, CancellationToken cancellationToken)
        {
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.WalletId == request.WalletId);
            if (wallet == null || wallet.WalletStatus == EnumWallet.IsLocked.ToString())
            {
                throw new KeyNotFoundException("Wallet not created or is locked.");
            }
            if(wallet.Point < request.Point)
            {
                throw new Exception("Insuffient wallet not enough.");
            }

            var transactionEntity = new Transaction()
            {
               
            };
        }*/
        
        public async Task<TransactionDto> DeleteTransactionAsync(int transId, CancellationToken cancellationToken)
        {
            var trans = await _uow.TransactionRepository.FirstOrDefaultAsync(x => x.TransactionId == transId);

            if(trans == null)
            {
                throw new KeyNotFoundException("Transaction not found.");
            }

            _uow.TransactionRepository.Delete(trans);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<TransactionDto>(trans);
        }

        public async Task<List<TransactionDto>> GetAll()
        {
            var trans = await _uow.TransactionRepository.GetAll();
            return _mapper.Map<List<TransactionDto>>(trans);    
        }

        public async Task<TransactionDto> GetTransactionByIdAsync(int transId, CancellationToken cancellationToken)
        {
            var result = await _uow.TransactionRepository.FirstOrDefaultAsync(x => x.TransactionId == transId);
            if(result == null)
            {
                throw new KeyNotFoundException("Transaction not found.");
            }

            return _mapper.Map<TransactionDto>(result);
        }

        public Task<List<TransactionDto>> GetTransactionByNameAsync(string transName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<List<TransactionDto>> GetTransactionByUserId(int userId, CancellationToken cancellationToken)
        {
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            if (wallet == null) throw new KeyNotFoundException("Wallet not found!");

            var transactions = await _uow.TransactionRepository
                .WhereAsync(x => x.WalletId == wallet.WalletId);

            var sortedTransactions = transactions.OrderByDescending(x => x.CreatedAt);

            return _mapper.Map<List<TransactionDto>>(sortedTransactions);
        }



        public async Task<TransactionDto> UpdateStatusTransactionAsync(int transId, EnumTransaction status, CancellationToken cancellationToken)
        {
            var transaction = await _uow.TransactionRepository.FirstOrDefaultAsync(x => x.TransactionId == transId);

            if(transaction == null)
            {
                throw new KeyNotFoundException("Transation not found.");
            }

            transaction.TransStatus = status.ToString();
            var result = _uow.TransactionRepository.Update(transaction);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<TransactionDto>(result);

        }

        public async Task<TransactionDto> UpdateTransactionAsync(int transId, UpdateTransactionRequest request, CancellationToken cancellationToken)
        {
            var tran = await _uow.TransactionRepository.FirstOrDefaultAsync(x => x.TransactionId == transId);
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.WalletId == request.WalletId);

            if (wallet == null || wallet.WalletStatus == EnumWallet.IsLocked.ToString())
            {
                throw new KeyNotFoundException("Wallet not created or is locked.");
            }
            if (tran == null)
            {
                throw new KeyNotFoundException("Transaction not found.");
            }

            tran.TransDesription = request.TransDesription;
            tran.TransStatus = request.TransStatus;
            tran.Point = request.Point;

            var result = _uow.TransactionRepository.Update(tran);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<TransactionDto>(result);
        }
    }
}
