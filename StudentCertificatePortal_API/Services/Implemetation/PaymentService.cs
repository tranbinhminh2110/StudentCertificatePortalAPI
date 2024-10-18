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
            if(result == null)
            {
                throw new KeyNotFoundException("Payment not found.");
            }

            return _mapper.Map<PaymentDto>(result); 
        }

        public async Task<PaymentDto> ProcessPayment(CreatePaymentRequest request, CancellationToken cancellation)
        {
            var result = new Payment();
            if(request.CourseEnrollmentId > 0)
            {
                // Process Payment of Course Enrollment
                // Check the enrollment and include related StudentOfCourses to calculate the total fee
                var enrollCourses = await _uow.CourseEnrollmentRepository.FirstOrDefaultAsync(
                    x => x.CourseEnrollmentId == request.CourseEnrollmentId,
                    cancellation,
                    include: p => p.Include(q => q.StudentOfCourses));

                if (enrollCourses == null)
                {
                    throw new KeyNotFoundException("Course Enrollment not found.");
                }

                // Check if the wallet has sufficient balance
                bool canPay = await CanPay(request.UserId, enrollCourses.TotalPrice ?? 0);

                if (!canPay)
                {
                    throw new Exception("Insufficient balance in wallet.");
                }

                // Retrieve the wallet
                var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId);
                if (wallet == null)
                {
                    throw new KeyNotFoundException("Wallet not found.");
                }

                var paymentEntity = new Payment()
                {
                    PaymentDate = DateTime.UtcNow,
                    PaymentPoint = enrollCourses.TotalPrice,
                    PaymentMethod = EnumTransaction.Success.ToString(),
                    WalletId = wallet.WalletId,
                    CourseEnrollmentId = request.CourseEnrollmentId,
                };

                result = await _uow.PaymentRepository.AddAsync(paymentEntity);
                await _uow.Commit(cancellation);

                wallet.Point -= enrollCourses.TotalPrice ?? 0;

                _uow.WalletRepository.Update(wallet);
                await _uow.Commit(cancellation);


                var enrollment = await _uow.CourseEnrollmentRepository.FirstOrDefaultAsync(x => x.CourseEnrollmentId == request.CourseEnrollmentId);
                enrollment.CourseEnrollmentStatus = EnumCourseEnrollment.Completed.ToString();

                _uow.CourseEnrollmentRepository.Update(enrollment);
                await _uow.Commit(cancellation);

                var socs = await _uow.StudentOfCourseRepository.WhereAsync(x => x.CouseEnrollmentId == request.CourseEnrollmentId);


                foreach(var soc in socs)
                {
                    soc.Status = true;
                    _uow.StudentOfCourseRepository.Update(soc);
                    await _uow.Commit(cancellation);

                }
               

            }
            else if( request.ExamEnrollmentId > 0)
            {
                // Process Payment of Exam Enrollment
                // Check info of exam enrollment 
                var enrollExams = await _uow.ExamEnrollmentRepository.FirstOrDefaultAsync(
                    x => x.ExamEnrollmentId == request.ExamEnrollmentId,
                    cancellation,
                    include: ee => ee.Include(q => q.StudentOfExams));
                 
                if(enrollExams == null)
                {
                    throw new KeyNotFoundException("Exam Enrollment not found.");
                }
                // Insufficient balance in wallet
                bool canPay = await CanPay(request.UserId, enrollExams.TotalPrice ?? 0);

                if (!canPay)
                {
                    throw new Exception("Insufficient balance in wallet.");
                }

                var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId);
                if (wallet == null)
                {
                    throw new KeyNotFoundException("Wallet not found."); ;
                }
                //Save info payment
                var paymentEntity = new Payment()
                {
                    PaymentDate = DateTime.UtcNow,
                    PaymentPoint = enrollExams.TotalPrice,
                    PaymentMethod = EnumTransaction.Success.ToString(),
                    WalletId = wallet.WalletId,
                    ExamEnrollmentId = request.ExamEnrollmentId,
                };

                result = await _uow.PaymentRepository.AddAsync(paymentEntity);
                await _uow.Commit(cancellation);
                // Update Point in wallet
                wallet.Point -= enrollExams.TotalPrice??0;
                
                
                _uow.WalletRepository.Update(wallet);
                await _uow.Commit(cancellation);

                var enrollment = await _uow.ExamEnrollmentRepository.FirstOrDefaultAsync( x=> x.ExamEnrollmentId == request.ExamEnrollmentId);
                
                enrollment.ExamEnrollmentStatus = EnumExamEnrollment.Completed.ToString();
                
                _uow.ExamEnrollmentRepository.Update(enrollment);
                await _uow.Commit(cancellation);    

                var soes = await _uow.StudentOfExamRepository.WhereAsync(x => x.EnrollmentId ==  enrollment.ExamEnrollmentId);
                foreach(var soe in soes)
                {
                    soe.Status = true;
                    _uow.StudentOfExamRepository.Update(soe);
                    await _uow.Commit(cancellation);
                }
            }
            // Returns
            return _mapper.Map<PaymentDto>(result);

        }

        // Kiểm tra số dư trong ví
        public async Task<bool> CanPay(int? userId, int? pointRequest)
        {
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == userId);

            if(wallet == null || wallet.WalletStatus == EnumWallet.IsLocked.ToString())
            {
                throw new KeyNotFoundException("Wallet not found or is locked.");

            }

            return wallet.Point >= pointRequest;

        }
    }
}
