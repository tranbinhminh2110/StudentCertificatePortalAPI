using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
using System.Xml.XPath;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public PaymentService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<List<PaymentDto>> GetAll()
        {
            var result = await _uow.PaymentRepository.GetAll();
            return _mapper.Map<List<PaymentDto>>(result);
        }

        public async Task<PaymentDto> GetPaymentByIdAsync(int paymentId)
        {
            var result = await _uow.PaymentRepository.FirstOrDefaultAsync(x => x.PaymentId == paymentId);
            if (result == null)
            {
                throw new KeyNotFoundException("Payment not found.");
            }

            return _mapper.Map<PaymentDto>(result);
        }



        public async Task<PaymentDto> ProcessPayment(CreatePaymentRequest request, CancellationToken cancellation)
        {
            var result = new Payment();
            if (request.ExamEnrollmentId > 0)
            {
                // Lấy thông tin Exam Enrollment
                var enrollExams = await _uow.ExamEnrollmentRepository.FirstOrDefaultAsync(
                    x => x.ExamEnrollmentId == request.ExamEnrollmentId,
                    cancellation,
                    include: ee => ee.Include(q => q.StudentOfExams));

                if (enrollExams == null)
                {
                    throw new KeyNotFoundException("Exam Enrollment not found.");
                }

                if (enrollExams.ExamEnrollmentStatus == Enums.EnumExamEnrollment.Completed.ToString())
                {
                    throw new Exception("The exam enrollment is already completed. You cannot modify or re-enroll.");
                }

                // Lấy danh sách ExamId đã mua
                var purchasedExamIds = enrollExams.StudentOfExams
                    .Where(soe => soe.Status == true) // Chỉ lấy các Exam đã được mua
                    .Select(soe => soe.ExamId)
                    .ToList();

                // Danh sách Exam chưa mua
                var examsToPurchase = enrollExams.StudentOfExams
                    .Where(soe => !purchasedExamIds.Contains(soe.ExamId))
                    .ToList();

                if (!examsToPurchase.Any())
                {
                    throw new Exception("All exams in this enrollment have already been purchased.");
                }

                // Tính tổng giá của các Exam cần mua
                var totalPriceToPay = examsToPurchase.Sum(soe => soe.Price ?? 0);

                // Kiểm tra số dư trong ví
                bool canPay = await CanPay(request.UserId, totalPriceToPay);
                if (!canPay)
                {
                    throw new Exception("Insufficient balance in wallet.");
                }

                // Lấy ví của người dùng
                var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId);
                if (wallet == null)
                {
                    throw new KeyNotFoundException("Wallet not found.");
                }

                // Thêm thông tin thanh toán
                var paymentEntity = new Payment()
                {
                    PaymentDate = DateTime.UtcNow,
                    PaymentPoint = totalPriceToPay,
                    PaymentMethod = "Using Point",
                    PaymentStatus = EnumTransaction.Success.ToString(),
                    WalletId = wallet.WalletId,
                    ExamEnrollmentId = request.ExamEnrollmentId,
                };

                result = await _uow.PaymentRepository.AddAsync(paymentEntity);
                await _uow.Commit(cancellation);

                // Cập nhật số dư trong ví
                wallet.Point -= totalPriceToPay;
                _uow.WalletRepository.Update(wallet);
                await _uow.Commit(cancellation);

                // Cập nhật trạng thái các Exam vừa được mua
                foreach (var exam in examsToPurchase)
                {
                    exam.Status = true;
                    exam.CreationDate = DateTime.UtcNow;
                    exam.ExpiryDate = DateTime.UtcNow.AddDays(3);
                    _uow.StudentOfExamRepository.Update(exam);
                }

                // Cập nhật trạng thái Enrollment
                enrollExams.ExamEnrollmentStatus = EnumExamEnrollment.Completed.ToString();
                _uow.ExamEnrollmentRepository.Update(enrollExams);
                await _uow.Commit(cancellation);
            }
            else
            {
                throw new ArgumentException("Invalid request: No ExamEnrollmentId provided.");
            }

            return _mapper.Map<PaymentDto>(result);
        }


        // Kiểm tra số dư trong ví
        public async Task<bool> CanPay(int? userId, int? pointRequest)
        {
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == userId);

            if (wallet == null || wallet.WalletStatus == EnumWallet.IsLocked.ToString())
            {
                throw new KeyNotFoundException("Wallet not found or is locked.");

            }

            return wallet.Point >= pointRequest;

        }

        public async Task<List<PaymentDto>> GetEEnrollPaymentByUserIdAsync(int userId)
        {
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            var payments = await _uow.PaymentRepository.WhereAsync(x => x.WalletId == wallet.WalletId && x.ExamEnrollmentId != null);

            return _mapper.Map<List<PaymentDto>>(payments);

        }

        public async Task<List<PaymentDto>> GetCEnrollPaymentByUserIdAsync(int userId)
        {
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            var payments = await _uow.PaymentRepository.WhereAsync(x => x.WalletId == wallet.WalletId && x.CourseEnrollmentId != null);

            return _mapper.Map<List<PaymentDto>>(payments);
        }
    }
}
