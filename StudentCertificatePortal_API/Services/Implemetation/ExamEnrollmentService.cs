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

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class ExamEnrollmentService : IExamEnrollmentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateExamEnrollmentRequest> _addExamEnrollmentValidator;
        private readonly IValidator<UpdateExamEnrollmentRequest> _updateExamEnrollmentValidator;


        public ExamEnrollmentService(IUnitOfWork uow, IMapper mapper,
            IValidator<CreateExamEnrollmentRequest> addExamEnrollmentValidator,
            IValidator<UpdateExamEnrollmentRequest> updateExamEnrollmentValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _addExamEnrollmentValidator = addExamEnrollmentValidator;
            _updateExamEnrollmentValidator = updateExamEnrollmentValidator;
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

            // Check if any existing enrollments match the requested exams
            var existingExams = userEnrollments
                .SelectMany(enrollment => enrollment.StudentOfExams)
                .Where(studentExam => request.Simulation_Exams.Contains(studentExam.ExamId))
                .ToList();

            // Remove old enrollments if they are incomplete or unpaid
            foreach (var exam in existingExams)
            {
                if (exam.Status == false) // Status is false for unpaid/incomplete
                {
                    // Tìm enrollment cũ liên kết với exam
                    var oldEnrollment = userEnrollments.FirstOrDefault(e => e.StudentOfExams.Any(se => se.ExamId == exam.ExamId));

                    if (oldEnrollment != null)
                    {
                        // Xóa từng StudentOfExam liên kết với exam
                        var examsToRemove = oldEnrollment.StudentOfExams.Where(se => se.ExamId == exam.ExamId).ToList();

                        foreach (var studentExam in examsToRemove)
                        {
                            oldEnrollment.StudentOfExams.Remove(studentExam);
                        }

                        // Xóa enrollment nếu không còn StudentOfExam nào
                        if (!oldEnrollment.StudentOfExams.Any())
                        {
                            _uow.ExamEnrollmentRepository.Delete(oldEnrollment);
                        }
                    }
                }

                else
                {
                    // Exam đã được mua hoặc hoàn tất
                    return new ExamEnrollmentResponse
                    {
                        IsEnrolled = true,
                        Status = "Purchased",
                        ExamEnrollmentId = exam.EnrollmentId,
                        Message = $"The user has already purchased the simulation exam with ExamId {exam.ExamId}."
                    };
                }
            }
            await _uow.Commit(cancellationToken);


            // Create new enrollment
            var simulations = new List<SimulationExam>();
            int totalPrice = 0;

            foreach (var simulationId in request.Simulation_Exams)
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

            // Add each exam to the StudentOfExam table
            foreach (var simulation in simulations)
            {
                var studentOfExamEntity = new StudentOfExam()
                {
                    Price = simulation.ExamDiscountFee,
                    Status = false, // Enrolled status
                    ExamId = simulation.ExamId,
                    EnrollmentId = enrollmentResult.ExamEnrollmentId
                };

                await _uow.StudentOfExamRepository.AddAsync(studentOfExamEntity);
            }

            await _uow.Commit(cancellationToken);

            return new ExamEnrollmentResponse
            {
                IsEnrolled = false,
                Status = "Created",
                ExamEnrollmentId = enrollmentResult.ExamEnrollmentId,
                Message = "Exam enrollment successfully created. Any previous unpaid enrollments have been removed.",
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
            if (eEnroll == null)
            {
                throw new KeyNotFoundException("Exam enrollment's User Id not found");
            }

            var examEnrollments = eEnroll.Select(ee => new ExamEnrollmentDto()
            {
                ExamEnrollmentId = ee.ExamEnrollmentId,
                ExamEnrollmentDate = ee.ExamEnrollmentDate,
                ExamEnrollmentStatus = ee.ExamEnrollmentStatus,
                TotalPrice = ee.TotalPrice,
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


    }
}
