﻿using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
using System.Xml.XPath;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;

        public PaymentService(IUnitOfWork uow, IMapper mapper, IHubContext<NotificationHub> hubContext, INotificationService notificationService)
        {
            _uow = uow;
            _mapper = mapper;
            _hubContext = hubContext;
            _notificationService = notificationService;
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
                    .Where(soe => soe.Status == "Completed") // Chỉ lấy các Exam đã được mua
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

                // Áp dụng TotalPriceVoucher nếu có
                if (enrollExams.TotalPriceVoucher.HasValue)
                {
                    totalPriceToPay = enrollExams.TotalPriceVoucher.Value;
                }

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
                    exam.Status = "Completed";
                    exam.CreationDate = DateTime.UtcNow;
                    exam.ExpiryDate = DateTime.UtcNow.AddDays(3);
                    _uow.StudentOfExamRepository.Update(exam);
                }

                // Cập nhật trạng thái Enrollment
                enrollExams.ExamEnrollmentStatus = EnumExamEnrollment.Completed.ToString();
                _uow.ExamEnrollmentRepository.Update(enrollExams);
                await _uow.Commit(cancellation);
                if (request.UserId.HasValue)
                {
                    await UpdateUserLevelAsync(request.UserId.Value, cancellation);
                }
                else
                {
                    throw new Exception("Invalid UserId");
                }
            }
            else if (request.CourseEnrollmentId > 0)
            {
                // Lấy thông tin Course Enrollment
                var courseEnrollment = await _uow.CourseEnrollmentRepository.FirstOrDefaultAsync(
                    x => x.CourseEnrollmentId == request.CourseEnrollmentId,
                    cancellation,
                    include: ce => ce.Include(q => q.StudentOfCourses));

                if (courseEnrollment == null)
                {
                    throw new KeyNotFoundException("Course Enrollment not found.");
                }

                if (courseEnrollment.CourseEnrollmentStatus == Enums.EnumCourseEnrollment.Completed.ToString())
                {
                    throw new Exception("The course enrollment is already completed. You cannot modify or re-enroll.");
                }

                // Danh sách các khóa học chưa được mua (Status == false)
                var coursesToPurchase = courseEnrollment.StudentOfCourses
                    .Where(sc => sc.Status == false) // Status là bool (true/false)
                    .ToList();

                if (!coursesToPurchase.Any())
                {
                    throw new Exception("All courses in this enrollment have already been purchased.");
                }

                // Tính tổng giá của các khóa học cần mua
                var totalPriceToPay = coursesToPurchase.Sum(sc => sc.Price ?? 0);

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
                    CourseEnrollmentId = request.CourseEnrollmentId,
                };

                result = await _uow.PaymentRepository.AddAsync(paymentEntity);
                await _uow.Commit(cancellation);

                // Cập nhật số dư trong ví
                wallet.Point -= totalPriceToPay;
                _uow.WalletRepository.Update(wallet);
                await _uow.Commit(cancellation);

                // Cập nhật trạng thái các khóa học vừa được mua
                foreach (var course in coursesToPurchase)
                {
                    course.Status = true; // Cập nhật trạng thái thành đã mua
                    course.CreationDate = DateTime.UtcNow;
                    course.ExpiryDate = DateTime.UtcNow.AddDays(30); // Ví dụ: hạn 30 ngày
                    _uow.StudentOfCourseRepository.Update(course);
                }

                // Cập nhật trạng thái của CourseEnrollment
                courseEnrollment.CourseEnrollmentStatus = EnumCourseEnrollment.Completed.ToString();
                _uow.CourseEnrollmentRepository.Update(courseEnrollment);
                await _uow.Commit(cancellation);

                if (request.UserId.HasValue)
                {
                    await UpdateUserLevelAsync(request.UserId.Value, cancellation);
                }
                else
                {
                    throw new Exception("Invalid UserId");
                }

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
        public async Task<PaymentPointDto> UpdateUserLevelAsync(int userId, CancellationToken cancellationToken)
        {
            // Lấy thông tin User
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            // Lấy Wallet tương ứng với User
            var wallet = await _uow.WalletRepository.FirstOrDefaultAsync(x => x.UserId == userId);
            if (wallet == null)
            {
                throw new KeyNotFoundException("Wallet not found.");
            }

            // Lấy danh sách Payments
            var payments = await _uow.PaymentRepository.WhereAsync(p => p.WalletId == wallet.WalletId);

            // Tính tổng PaymentPoint
            var totalPaymentPoints = payments.Sum(p => p.PaymentPoint);

            var oldUserLevel = user.UserLevel;

            // Xác định UserLevel dựa trên PaymentPoint
            if (totalPaymentPoints >= 500)
            {
                user.UserLevel = EnumLevel.Diamond.ToString();
            }
            else if (totalPaymentPoints >= 300)
            {
                user.UserLevel = EnumLevel.Gold.ToString();
            }
            else if (totalPaymentPoints >= 100)
            {
                user.UserLevel = EnumLevel.Silver.ToString();
            }
            else
            {
                user.UserLevel = EnumLevel.Bronze.ToString();
            }

            if (user.UserLevel != oldUserLevel)
            {
                var notification = new Notification()
                {
                    NotificationName = $"Congratulations you have been promoted to {user.UserLevel} membership",
                    NotificationDescription = $"Congratulations you have been promoted to {user.UserLevel} membership, after reaching the spending milestone of {totalPaymentPoints} coins.",
                    NotificationImage = user.UserImage,
                    CreationDate = DateTime.UtcNow,
                    Role = "Student",
                    IsRead = false,
                    UserId = user.UserId,
                };

                // Thêm thông báo vào cơ sở dữ liệu
                await _uow.NotificationRepository.AddAsync(notification);
                await _uow.Commit(cancellationToken);

                // Gửi thông báo qua SignalR (nếu cần)
                var notifications = await _notificationService.GetNotificationByRoleAsync("Student", cancellationToken);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);
            }

            // Cập nhật User trong Database
            _uow.UserRepository.Update(user);
            await _uow.Commit(cancellationToken);

      
            var userLevelDto = new PaymentPointDto
            {
                User = _mapper.Map<UserDto>(user),  // Chuyển đổi thông tin User sang UserDto
                TotalPaymentPoints = totalPaymentPoints ?? 0
            };

            return userLevelDto;
        }



    }
}
