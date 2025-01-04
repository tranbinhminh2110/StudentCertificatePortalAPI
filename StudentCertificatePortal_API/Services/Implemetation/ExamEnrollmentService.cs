using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.Contracts.Responses;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
using System.Threading;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class ExamEnrollmentService : IExamEnrollmentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IVoucherService _voucherService;


        private readonly IValidator<CreateExamEnrollmentRequest> _addExamEnrollmentValidator;
        private readonly IValidator<UpdateExamEnrollmentRequest> _updateExamEnrollmentValidator;

        private readonly IValidator<CreateExamEnrollmentVoucherRequest> _addExamEnrollmentVoucherValidator;
        private readonly IValidator<UpdateExamEnrollmentVoucherRequest> _updateExamEnrollmentVoucherValidator;

        public ExamEnrollmentService(IUnitOfWork uow, IMapper mapper,
            IValidator<CreateExamEnrollmentRequest> addExamEnrollmentValidator,
            IValidator<UpdateExamEnrollmentRequest> updateExamEnrollmentValidator,
            IValidator<CreateExamEnrollmentVoucherRequest> addExamEnrollmentVoucherValidator,
            IValidator<UpdateExamEnrollmentVoucherRequest> updateExamEnrollmentVoucherValidator,
            IVoucherService voucherService)
        {
            _uow = uow;
            _mapper = mapper;
            _addExamEnrollmentValidator = addExamEnrollmentValidator;
            _updateExamEnrollmentValidator = updateExamEnrollmentValidator;
            _addExamEnrollmentVoucherValidator = addExamEnrollmentVoucherValidator;
            _updateExamEnrollmentVoucherValidator = updateExamEnrollmentVoucherValidator;
            _voucherService = voucherService;
        }
        public async Task<ExamEnrollmentResponse> CreateExamEnrollmentAsync(CreateExamEnrollmentRequest request, CancellationToken cancellationToken)
        {
            // Validate request using validator
            var validation = await _addExamEnrollmentValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            // Check if user exists
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found. Exam Enrollment creation requires a valid UserId.");
            }

            if (request.Simulation_Exams == null || !request.Simulation_Exams.Any())
            {
                throw new ArgumentException("Simulation_Exams cannot be null or empty.");
            }

            // Fetch existing enrollments for the user
            var userEnrollments = await _uow.ExamEnrollmentRepository
                .Include(x => x.StudentOfExams)
                .Where(x => x.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            // Get all StudentOfExams for this user
            var existingStudentExams = userEnrollments
                .SelectMany(e => e.StudentOfExams)
                .ToList();

            // Check for duplicate exams that are unpaid or expired and remove them
            var duplicateExamsToRemove = existingStudentExams
                .Where(exam =>
                    request.Simulation_Exams.Contains(exam.ExamId) &&
                    (exam.Status == "Unpaid" || exam.Status == "Expired"))
                .ToList();

            foreach (var examToRemove in duplicateExamsToRemove)
            {
                var enrollment = userEnrollments.First(e => e.ExamEnrollmentId == examToRemove.EnrollmentId);
                enrollment.StudentOfExams.Remove(examToRemove);

                // Mark enrollment as Cancelled if it becomes empty
                if (!enrollment.StudentOfExams.Any())
                {
                    enrollment.ExamEnrollmentStatus = "Cancelled";
                }
                // Recalculate total price for the enrollment
                else
                {
                    enrollment.TotalPrice = enrollment.StudentOfExams.Sum(e => e.Price ?? 0);
                }

                _uow.ExamEnrollmentRepository.Update(enrollment);
            }
            await _uow.Commit(cancellationToken);

            // Check for already purchased (valid) exams
            var purchasedExamIds = existingStudentExams
                .Where(exam => exam.Status != "Unpaid" && exam.Status != "Expired")
                .Select(exam => exam.ExamId)
                .ToList();

            // Remove purchased exams from the request
            var remainingExams = request.Simulation_Exams
                .Where(examId => !purchasedExamIds.Contains(examId))
                .ToList();

            // If all exams are already purchased
            if (!remainingExams.Any())
            {
                return new ExamEnrollmentResponse
                {
                    IsEnrolled = true,
                    Status = "Purchased",
                    ExamEnrollmentId = userEnrollments
                        .SelectMany(e => e.StudentOfExams)
                        .First(e => purchasedExamIds.Contains(e.ExamId))
                        .EnrollmentId,
                    Message = "All requested simulation exams have already been purchased."
                };
            }

            // Create new enrollment for remaining exams
            var simulations = new List<SimulationExam>();
            int totalPrice = 0;

            foreach (var simulationId in remainingExams)
            {
                var simulation = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == simulationId, cancellationToken);
                if (simulation == null)
                {
                    throw new KeyNotFoundException($"Simulation Id {simulationId} not found.");
                }

                if (simulation.ExamDiscountFee == null)
                {
                    simulation.ExamDiscountFee = simulation.ExamFee;
                    _uow.SimulationExamRepository.Update(simulation);
                    await _uow.Commit(cancellationToken);
                }

                simulations.Add(simulation);
                totalPrice += simulation.ExamDiscountFee ?? 0;
            }

            var newEnrollmentEntity = new ExamsEnrollment()
            {
                UserId = request.UserId,
                ExamEnrollmentDate = DateTime.UtcNow,
                ExamEnrollmentStatus = EnumExamEnrollment.OnGoing.ToString(),
                TotalPrice = totalPrice
            };

            var enrollmentResult = await _uow.ExamEnrollmentRepository.AddAsync(newEnrollmentEntity);
            await _uow.Commit(cancellationToken);

            // Add each remaining exam to the StudentOfExam table
            foreach (var simulation in simulations)
            {
                var studentOfExamEntity = new StudentOfExam()
                {
                    Price = simulation.ExamDiscountFee,
                    Status = "Unpaid",
                    ExamId = simulation.ExamId,
                    EnrollmentId = enrollmentResult.ExamEnrollmentId
                };

                await _uow.StudentOfExamRepository.AddAsync(studentOfExamEntity);
            }

            await _uow.Commit(cancellationToken);

            var responseMessage = purchasedExamIds.Any()
                ? "New enrollment created for unpurchased exams. Some requested exams were already purchased."
                : "New exam enrollment created. Previous unpaid enrollments have been marked as cancelled.";

            return new ExamEnrollmentResponse
            {
                IsEnrolled = false,
                Status = "Created",
                ExamEnrollmentId = enrollmentResult.ExamEnrollmentId,
                Message = responseMessage,
                ExamEnrollment = _mapper.Map<ExamEnrollmentDto>(enrollmentResult)
            };
        }

        public async Task<ExamEnrollmentResponse> CreateExamEnrollmentVoucherAsync(CreateExamEnrollmentVoucherRequest request, CancellationToken cancellationToken)
        {
            // Validate request using validator
            var validation = await _addExamEnrollmentVoucherValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            // Check if user exists
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found. Exam Enrollment creation requires a valid UserId.");
            }

            if (request.Simulation_Exams == null || !request.Simulation_Exams.Any())
            {
                throw new ArgumentException("Simulation_Exams cannot be null or empty.");
            }

            // Fetch existing enrollments for the user
            var userEnrollments = await _uow.ExamEnrollmentRepository
                .Include(x => x.StudentOfExams)
                .Where(x => x.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            // Get all StudentOfExams for this user that are either unpaid or expired
            var existingStudentExams = userEnrollments
                .SelectMany(e => e.StudentOfExams)
                .ToList();

            var duplicateExamsToRemove = existingStudentExams
                .Where(exam =>
                    request.Simulation_Exams.Contains(exam.ExamId) &&
                    (exam.Status == "Unpaid" || exam.Status == "Expired"))
                .ToList();

            // Remove duplicate exams from their enrollments
            foreach (var examToRemove in duplicateExamsToRemove)
            {
                var enrollment = userEnrollments.First(e => e.ExamEnrollmentId == examToRemove.EnrollmentId);
                enrollment.StudentOfExams.Remove(examToRemove);

                // Mark enrollment as Cancelled if it becomes empty
                if (!enrollment.StudentOfExams.Any())
                {
                    enrollment.ExamEnrollmentStatus = "Cancelled";
                }
                // Recalculate total price for the enrollment
                else
                {
                    enrollment.TotalPrice = enrollment.StudentOfExams.Sum(e => e.Price ?? 0);
                }

                _uow.ExamEnrollmentRepository.Update(enrollment);
            }
            await _uow.Commit(cancellationToken);

            // Check for already purchased (valid) exams
            var purchasedExamIds = existingStudentExams
                .Where(exam => exam.Status != "Unpaid" && exam.Status != "Expired")
                .Select(exam => exam.ExamId)
                .ToList();

            // Remove purchased exams from the request
            var remainingExams = request.Simulation_Exams
                .Where(examId => !purchasedExamIds.Contains(examId))
                .ToList();

            // If all exams are already purchased
            if (!remainingExams.Any())
            {
                return new ExamEnrollmentResponse
                {
                    IsEnrolled = true,
                    Status = "Purchased",
                    ExamEnrollmentId = userEnrollments
                        .SelectMany(e => e.StudentOfExams)
                        .First(e => purchasedExamIds.Contains(e.ExamId))
                        .EnrollmentId,
                    Message = "All requested simulation exams have already been purchased."
                };
            }

            // Find matching enrollment for remaining exams
            var matchingEnrollment = userEnrollments
                .Where(enrollment =>
                    enrollment.StudentOfExams.All(exam => exam.Status == "Unpaid") && // All exams must be unpaid
                    enrollment.StudentOfExams.Count == remainingExams.Count && // Must have same count
                    enrollment.StudentOfExams.All(exam => remainingExams.Contains(exam.ExamId))) // All exams must match
                .FirstOrDefault();

            // If we found a matching enrollment with exact exam count, reuse it
            if (matchingEnrollment != null)
            {
                // Update the enrollment date
                matchingEnrollment.TotalPrice = matchingEnrollment.StudentOfExams.Sum(e => e.Price ?? 0);
                matchingEnrollment.ExamEnrollmentDate = DateTime.UtcNow;
                _uow.ExamEnrollmentRepository.Update(matchingEnrollment);
                await _uow.Commit(cancellationToken);

                var message = purchasedExamIds.Any()
                    ? "Some exams were already purchased. Existing unpaid enrollment reused for remaining exams."
                    : "Existing unpaid enrollment reused.";

                return new ExamEnrollmentResponse
                {
                    IsEnrolled = false,
                    Status = "Created",
                    ExamEnrollmentId = matchingEnrollment.ExamEnrollmentId,
                    Message = message,
                    ExamEnrollment = _mapper.Map<ExamEnrollmentDto>(matchingEnrollment)
                };
            }

            // Update status of old unpaid enrollments to "Cancelled"
            foreach (var enrollment in userEnrollments)
            {
                if (enrollment.StudentOfExams.All(exam => exam.Status == "Unpaid"))
                {
                    enrollment.ExamEnrollmentStatus = "Cancelled";
                    _uow.ExamEnrollmentRepository.Update(enrollment);
                }
            }
            await _uow.Commit(cancellationToken);

            // Create new enrollment for remaining exams
            var simulations = new List<SimulationExam>();
            int totalPrice = 0;
            float totalVoucherDiscount = 0f;

            foreach (var voucherId in request.VoucherIds)
            {
                if (voucherId > 0)
                {
                    var voucher = await _voucherService.GetVoucherByIdAsync(voucherId, cancellationToken);
                    if (voucher == null || voucher.VoucherStatus == false)
                    {
                        throw new KeyNotFoundException($"Voucher with ID {voucherId} not found or is expired.");
                    }


                    totalVoucherDiscount += (float)voucher.Percentage;

                }
            }
            int totalPriceVoucher = 0;

            foreach (var simulationId in remainingExams)
            {
                var simulation = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == simulationId, cancellationToken);
                if (simulation == null)
                {
                    throw new KeyNotFoundException($"Simulation Id {simulationId} not found.");
                }

                if (simulation.ExamFee.HasValue)
                {
                    totalPrice += simulation.ExamFee.Value;  // Cộng giá gốc của bài thi vào tổng
                }
                if (simulation.ExamFee.HasValue && totalVoucherDiscount > 0)
                {
                    float discountAmount = totalPrice * (totalVoucherDiscount / 100f);
                    totalPriceVoucher = (int)Math.Ceiling(totalPrice - discountAmount);
                }
                else
                {
                    totalPriceVoucher = totalPrice;
                }

                simulations.Add(simulation);

            }

            var newEnrollmentEntity = new ExamsEnrollment()
            {
                UserId = request.UserId,
                ExamEnrollmentDate = DateTime.UtcNow,
                ExamEnrollmentStatus = EnumExamEnrollment.OnGoing.ToString(),
                TotalPrice = totalPrice,
                TotalPriceVoucher = totalPriceVoucher

            };

            var enrollmentResult = await _uow.ExamEnrollmentRepository.AddAsync(newEnrollmentEntity);
            await _uow.Commit(cancellationToken);

            // Add each remaining exam to the StudentOfExam table
            foreach (var simulation in simulations)
            {
                var studentOfExamEntity = new StudentOfExam()
                {
                    Price = simulation.ExamFee,
                    Status = "Unpaid",
                    ExamId = simulation.ExamId,
                    EnrollmentId = enrollmentResult.ExamEnrollmentId
                };

                await _uow.StudentOfExamRepository.AddAsync(studentOfExamEntity);
            }

            await _uow.Commit(cancellationToken);

            var responseMessage = purchasedExamIds.Any()
                ? "New enrollment created for unpurchased exams. Some requested exams were already purchased."
                : "New exam enrollment created. Previous unpaid enrollments have been marked as cancelled.";

            return new ExamEnrollmentResponse
            {
                IsEnrolled = false,
                Status = "Created",
                ExamEnrollmentId = enrollmentResult.ExamEnrollmentId,
                Message = responseMessage,
                ExamEnrollment = _mapper.Map<ExamEnrollmentDto>(enrollmentResult)
            };
        }

        public async Task<ExamEnrollmentDto> DeleteExamEnrollmentAsync(int examEnrollmentId, CancellationToken cancellationToken)
        {
            var examEnrollment = await _uow.ExamEnrollmentRepository.FirstOrDefaultAsync(
                x => x.ExamEnrollmentId == examEnrollmentId,
                cancellationToken,
                include: p => p.Include(q => q.StudentOfExams)
                               .Include(q => q.Payments));



            if (examEnrollment is null)
            {
                throw new KeyNotFoundException("Exam Enrollment not found.");
            }

            var userId = examEnrollment.UserId;
            var examId = examEnrollment.StudentOfExams.Select(x => x.ExamId).ToList();
            foreach (var exam in examId)
            {
                var score = await _uow.ScoreRepository.FirstOrDefaultAsync(x => x.UserId == userId && x.ExamId == exam, cancellationToken
                    , include: i => i.Include(sc => sc.PeerReviews));
                if (score != null)
                {
                    score.PeerReviews.Clear();
                    _uow.ScoreRepository.Delete(score);
                    await _uow.Commit(cancellationToken);
                }
            }

            examEnrollment?.StudentOfExams.Clear();
            examEnrollment?.Payments.Clear();
            _uow.ExamEnrollmentRepository.Delete(examEnrollment);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<ExamEnrollmentDto>(examEnrollment);
        }

        public async Task<List<ExamEnrollmentDto>> GetAll()
        {
            var result = await _uow.ExamEnrollmentRepository.GetAllAsync(
                include: q => q.Include(q => q.StudentOfExams).ThenInclude(se => se.Exam)
                );

            var examEnrollments = result.Select(ee => new ExamEnrollmentDto()
            {
                ExamEnrollmentId = ee.ExamEnrollmentId,
                ExamEnrollmentDate = ee.ExamEnrollmentDate,
                ExamEnrollmentStatus = ee.ExamEnrollmentStatus,
                TotalPrice = ee.TotalPrice,
                TotalPriceVoucher = ee.TotalPriceVoucher,
                UserId = ee.UserId,
                CreationDate = ee.StudentOfExams.FirstOrDefault() != null
               ? ee.StudentOfExams.FirstOrDefault().CreationDate
               : (DateTime?)null,
                ExpiryDate = ee.StudentOfExams.FirstOrDefault() != null
               ? ee.StudentOfExams.FirstOrDefault().ExpiryDate
               : (DateTime?)null,
                SimulationExamDetail = ee.StudentOfExams.Select(se => new ExamDetailsDto()
                {
                    ExamId = se.ExamId,
                    ExamCode = se.Exam.ExamCode,
                    ExamName = se.Exam.ExamName,
                    ExamDiscountFee = se.Exam.ExamDiscountFee,
                    ExamImage = se.Exam.ExamImage,
                    ExamFee = se.Exam.ExamFee,
                    ExamDescription = se.Exam.ExamDescription,
                    ExamPermission = se.Exam.ExamPermission

                }).ToList()
            }).ToList();

            return examEnrollments;
        }

        public async Task<ExamEnrollmentDto> GetExamEnrollmentById(int examEnrollmentId, CancellationToken cancellationToken)
        {
            var result = await _uow.ExamEnrollmentRepository
                .FirstOrDefaultAsync(x => x.ExamEnrollmentId == examEnrollmentId, cancellationToken,
                include: q => q.Include(ee => ee.StudentOfExams).ThenInclude(se => se.Exam));
            if (result is null)
            {
                throw new KeyNotFoundException("Exam Enrollment not found.");
            }


            var examEnrollment = new ExamEnrollmentDto()
            {
                ExamEnrollmentId = result.ExamEnrollmentId,
                ExamEnrollmentDate = result.ExamEnrollmentDate,
                ExamEnrollmentStatus = result.ExamEnrollmentStatus,
                TotalPrice = result.TotalPrice,
                TotalPriceVoucher = result.TotalPriceVoucher,
                UserId = result.UserId,
                CreationDate = result.StudentOfExams.FirstOrDefault() != null
               ? result.StudentOfExams.FirstOrDefault().CreationDate
               : (DateTime?)null,
                ExpiryDate = result.StudentOfExams.FirstOrDefault() != null
               ? result.StudentOfExams.FirstOrDefault().ExpiryDate
               : (DateTime?)null,
                SimulationExamDetail = result.StudentOfExams.Select(se => new ExamDetailsDto
                {
                    ExamId = se.ExamId,
                    ExamCode = se.Exam.ExamCode,
                    ExamName = se.Exam.ExamName,
                    ExamDiscountFee = se.Exam.ExamDiscountFee,
                    ExamImage = se.Exam.ExamImage,
                    ExamFee = se.Exam.ExamFee,
                    ExamDescription = se.Exam.ExamDescription,
                    ExamPermission = se.Exam.ExamPermission
                }).ToList()
            };
            return examEnrollment;
        }

        public async Task<List<ExamEnrollmentDto>> GetExamEnrollmentByUserId(int userId, CancellationToken cancellationToken)
        {
            var eEnroll = await _uow.ExamEnrollmentRepository.WhereAsync(x => x.UserId == userId, cancellationToken
                , include: q => q.Include(se => se.StudentOfExams).ThenInclude(x => x.Exam));
            if (eEnroll == null || !eEnroll.Any())
            {
                throw new KeyNotFoundException("No exam enrollments found for the provided User ID.");
            }
            // Check and update each enrollment
            foreach (var enroll in eEnroll)
            {
                // Update TotalPriceVoucher if the status is OnGoing or Expired
                if (enroll.ExamEnrollmentStatus == Enums.EnumExamEnrollment.OnGoing.ToString() ||
                    enroll.ExamEnrollmentStatus == Enums.EnumExamEnrollment.Expired.ToString())
                {
                    enroll.TotalPriceVoucher = enroll.TotalPrice;
                    _uow.ExamEnrollmentRepository.Update(enroll);
                }
                else if (enroll.ExamEnrollmentStatus != Enums.EnumExamEnrollment.Completed.ToString())
                {
                    await CheckExamInEnrollment(enroll, cancellationToken);
                }
            }

            // Save changes to the database
            await _uow.Commit(cancellationToken);

            // Check and update each enrollment
            foreach (var enroll in eEnroll)
            {
                if(enroll.ExamEnrollmentStatus != Enums.EnumExamEnrollment.Completed.ToString())
                {
                    var updatedEnrollment = await CheckExamInEnrollment(enroll, cancellationToken);
                }
            }
            var eEnrollAlter = await _uow.ExamEnrollmentRepository.WhereAsync(x => x.UserId == userId, cancellationToken
                , include: q => q.Include(se => se.StudentOfExams).ThenInclude(x => x.Exam));
            var examEnrollments = eEnrollAlter.Select(ee => new ExamEnrollmentDto()
            {
                ExamEnrollmentId = ee.ExamEnrollmentId,
                ExamEnrollmentDate = ee.ExamEnrollmentDate,
                ExamEnrollmentStatus = ee.ExamEnrollmentStatus,
                TotalPrice = ee.TotalPrice,
                TotalPriceVoucher = ee.TotalPriceVoucher,
                UserId = ee.UserId,
                CreationDate = ee.StudentOfExams.FirstOrDefault() != null
                ? ee.StudentOfExams.FirstOrDefault().CreationDate
                : (DateTime?)null,
                ExpiryDate = ee.StudentOfExams.FirstOrDefault() != null
                ? ee.StudentOfExams.FirstOrDefault().ExpiryDate
                : (DateTime?)null,
                SimulationExamDetail = ee.StudentOfExams.Select(se => new ExamDetailsDto()
                {
                    ExamId = se.ExamId,
                    ExamCode = se.Exam.ExamCode,
                    ExamName = se.Exam.ExamName,
                    ExamDiscountFee = se.Exam.ExamDiscountFee,
                    ExamImage = se.Exam.ExamImage,
                    ExamFee = se.Exam.ExamFee,
                    ExamDescription = se.Exam.ExamDescription,
                    ExamPermission = se.Exam.ExamPermission
                }).ToList()
            }).ToList();

            return _mapper.Map<List<ExamEnrollmentDto>>(examEnrollments);
        }

        public async Task<ExamEnrollmentDto> CheckExamInEnrollment(ExamsEnrollment enroll, CancellationToken cancellationToken)
        {
            int totalPrice = 0;
            var examIds = enroll.StudentOfExams.Select(soe => soe.ExamId).ToList();
            var exams = await _uow.SimulationExamRepository.WhereAsync(x => examIds.Contains(x.ExamId), cancellationToken);

            var examDictionary = exams.ToDictionary(x => x.ExamId);

            foreach(var soe in enroll.StudentOfExams)
            {
                if(examDictionary.TryGetValue(soe.ExamId, out var exam))
                {
                    if(exam.ExamDiscountFee != soe.Price)
                    {
                        soe.Price = exam.ExamDiscountFee;
                        totalPrice += exam.ExamDiscountFee ?? 0;
                    }
                    else
                    {
                        totalPrice += exam.ExamDiscountFee ?? 0;
                    }
                }
            }

            enroll.TotalPrice = totalPrice;
            var result = _uow.ExamEnrollmentRepository.Update(enroll);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<ExamEnrollmentDto>(result);
        }

        /*public async Task<List<ExamEnrollmentDto>> GetExamEnrollmentByNameAsync(string examEnrollmentName, CancellationToken cancellationToken)
        {
            var result = await _uow.ExamEnrollmentRepository.WhereAsync(x => x.Exa.Contains(examEnrollmentName), cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Course not found.");
            }
            return _mapper.Map<List<CourseDto>>(result);
        }*/

        public async Task<ExamEnrollmentDto> UpdateExamEnrollmentAsync(int examEnrollmentId, UpdateExamEnrollmentRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateExamEnrollmentValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            var exam = await _uow.ExamEnrollmentRepository.FirstOrDefaultAsync(x => x.ExamEnrollmentId == examEnrollmentId, cancellationToken);
            if (exam == null)
            {
                throw new KeyNotFoundException("Exam Enrollment not found.");
            }

            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found. Exam Enrollment update requires a valid UserId.");
            }

            if (request.Simulation_Exams == null || !request.Simulation_Exams.Any())
            {
                throw new ArgumentException("Simulation_Exams cannot be null or empty.");
            }

            // Khởi tạo totalPrice
            int? totalPrice = 0;

            var simulations = new List<SimulationExam>();
            foreach (var simulationId in request.Simulation_Exams)
            {
                var simulation = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == simulationId, cancellationToken);
                if (simulation != null)
                {
                    simulations.Add(simulation);
                }
            }

            // Tính toán tổng giá từ ExamDiscountFee của các SimulationExam
            foreach (var simulation in simulations)
            {
                if (simulation == null)
                {
                    throw new KeyNotFoundException($"Simulation exam with ID {simulation.ExamId} not found.");
                }

                // Cập nhật totalPrice với ExamDiscountFee
                totalPrice += simulation.ExamDiscountFee;
            }

            // Cập nhật thông tin ExamEnrollment
            exam.UserId = request.UserId;
            exam.TotalPrice = totalPrice > 0 ? totalPrice : 0; // Gán tổng giá
            exam.ExamEnrollmentDate = DateTime.UtcNow;

            // Lưu các thay đổi
            _uow.ExamEnrollmentRepository.Update(exam);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<ExamEnrollmentDto>(exam);
        }

        public async Task<ExamEnrollmentDto> UpdateExamEnrollmentVoucherAsync(int examEnrollmentId, UpdateExamEnrollmentVoucherRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateExamEnrollmentVoucherValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            var exam = await _uow.ExamEnrollmentRepository.FirstOrDefaultAsync(x => x.ExamEnrollmentId == examEnrollmentId, cancellationToken);
            if (exam == null)
            {
                throw new KeyNotFoundException("Exam Enrollment not found.");
            }

            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found. Exam Enrollment update requires a valid UserId.");
            }

            if (request.Simulation_Exams == null || !request.Simulation_Exams.Any())
            {
                throw new ArgumentException("Simulation_Exams cannot be null or empty.");
            }

            // Khởi tạo totalPrice và totalVoucherDiscount
            int totalPrice = 0;
            float totalVoucherDiscount = 0f;

            // Xử lý voucher
            foreach (var voucherId in request.VoucherIds)
            {
                if (voucherId > 0)
                {
                    var voucher = await _voucherService.GetVoucherByIdAsync(voucherId, cancellationToken);
                    if (voucher == null || voucher.VoucherStatus == false)
                    {
                        throw new KeyNotFoundException($"Voucher with ID {voucherId} not found or is expired.");
                    }
                    totalVoucherDiscount += (float)voucher.Percentage;
                }
            }

            // Xử lý SimulationExams
            var simulations = new List<SimulationExam>();
            foreach (var simulationId in request.Simulation_Exams)
            {
                var simulation = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == simulationId, cancellationToken);
                if (simulation == null)
                {
                    throw new KeyNotFoundException($"Simulation exam with ID {simulationId} not found.");
                }

                // Cộng giá gốc của bài thi vào tổng giá
                if (simulation.ExamFee.HasValue)
                {
                    totalPrice += simulation.ExamFee.Value;
                }

                simulations.Add(simulation);
            }

            // Tính toán tổng giá trị sau khi áp dụng voucher
            int totalPriceVoucher = totalPrice;
            if (totalVoucherDiscount > 0)
            {
                float discountAmount = totalPrice * (totalVoucherDiscount / 100f);
                totalPriceVoucher = (int)Math.Ceiling(totalPrice - discountAmount);
            }

            // Cập nhật thông tin ExamEnrollment
            exam.UserId = request.UserId;
            exam.TotalPrice = totalPrice;
            exam.TotalPriceVoucher = totalPriceVoucher;
            exam.ExamEnrollmentDate = DateTime.UtcNow;

            // Lưu các thay đổi
            _uow.ExamEnrollmentRepository.Update(exam);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<ExamEnrollmentDto>(exam);
        }
    }
}
