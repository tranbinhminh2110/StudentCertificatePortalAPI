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
            var validation = await _addExamEnrollmentValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);
            var userEnrollments = await _uow.ExamEnrollmentRepository.WhereAsync(x => x.UserId == request.UserId);
             /* Select all exam enrollment có cùng user*/
            var userEnrollmentIds = userEnrollments.Select(x => x.ExamEnrollmentId);

             /*
              - Kiểm tra số lượng simulation có trong enrollment đã tồn tại 
              */

            int count = 0;
            List<ExamsEnrollment> listEnroll = new List<ExamsEnrollment>();
            foreach (var examIdIndex in request.Simulation_Exams)
            {
                 /* Kiểm tra xem exam Enrollment có cùng exam id trong studenofexams và có cùng enrollment id
                  
                  */
                var enrollmentExist = await _uow.ExamEnrollmentRepository.Include(x => x.StudentOfExams)
                    .Where(a => a.StudentOfExams.Any(s => s.ExamId == examIdIndex && userEnrollmentIds.Contains(s.EnrollmentId))).ToListAsync();

                if (enrollmentExist != null)
                {
                    foreach(var index in enrollmentExist)
                    {
                        if (!listEnroll.Contains(index))
                        {
                            count++;
                            listEnroll.Add(index);
                        }
                        
                    }
                    
                }

            }

            if(count == 1)
            {
                foreach(var enrollmentExist in listEnroll)
                {
                    if (enrollmentExist.ExamEnrollmentStatus == Enums.EnumExamEnrollment.Completed.ToString())
                    {
                        return new ExamEnrollmentResponse
                        {
                            IsEnrolled = true,
                            Status = "Completed",
                            ExamEnrollmentId = enrollmentExist.ExamEnrollmentId,
                            Message = "The user is already enrolled in this simulation (Completed)."
                        };
                    }
                    else if (enrollmentExist.ExamEnrollmentStatus == Enums.EnumExamEnrollment.OnGoing.ToString())
                    {
                        return new ExamEnrollmentResponse
                        {
                            IsEnrolled = true,
                            Status = "OnGoing",
                            ExamEnrollmentId = enrollmentExist.ExamEnrollmentId,
                            Message = $"The user is currently enrolled in this simulation (OnGoing). Payment is required to continue."
                        };
                    }
                }
            }else if(count > 1)
            {
                foreach(var examIntoEnroll in listEnroll)
                {

                    var studentOfExamToReomve = examIntoEnroll.StudentOfExams.Where(s => request.Simulation_Exams.Contains(s.ExamId)).ToList();

                    foreach(var studentOfExam in studentOfExamToReomve)
                    {

                        // Load the enrollment along with its related StudentOfExams
                        var enrollment = await _uow.ExamEnrollmentRepository
                            .Include(e => e.StudentOfExams)
                            .FirstOrDefaultAsync(e => e.ExamEnrollmentId == studentOfExam.EnrollmentId, cancellationToken);

                        if (enrollment != null)
                        {
                            // Find the specific StudentOfExam entries to remove
                            var examsToRemove = enrollment.StudentOfExams
                                .Where(s => s.ExamId == studentOfExam.ExamId)
                                .ToList(); // Create a list to avoid modifying the collection while iterating

                            // Remove each entry found
                            foreach (var i in examsToRemove)
                            {
                                enrollment.StudentOfExams.Remove(i);
                            }

                            // Commit the changes to persist them
                            await _uow.Commit(cancellationToken);
                        }

                    }
                }
            }

            if (user == null)
            {
                throw new KeyNotFoundException("User not found. Exam Enrollment creation requires a valid UserId.");
            }

            if (request.Simulation_Exams == null || !request.Simulation_Exams.Any())
            {
                throw new ArgumentException("Simulation_Exams cannot be null or empty.");
            }

            int? totalPrice = 0;

            var simulations = new List<SimulationExam>();

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
            }

            var eEnrollmentEntity = new ExamsEnrollment()
            {
                UserId = request.UserId,
                ExamEnrollmentDate = DateTime.UtcNow,
                ExamEnrollmentStatus = EnumExamEnrollment.OnGoing.ToString(),
                TotalPrice = totalPrice > 0 ? totalPrice : 0, 
            };

            var result = await _uow.ExamEnrollmentRepository.AddAsync(eEnrollmentEntity);
            await _uow.Commit(cancellationToken);

            foreach (var simulation in simulations)
            {
                if (simulation == null)
                {
                    throw new KeyNotFoundException($"Simulation exam with ID {simulation.ExamId} not found.");
                }

                var studentOfExamEntity = new StudentOfExam()
                {
                    CreationDate = DateTime.Now,
                    Price = simulation.ExamDiscountFee,
                    Status = false,
                    ExamId = simulation.ExamId,
                    EnrollmentId = result.ExamEnrollmentId 
                };

                await _uow.StudentOfExamRepository.AddAsync(studentOfExamEntity);
                totalPrice += simulation.ExamDiscountFee;
            }
            result.TotalPrice = totalPrice > 0 ? totalPrice : 0;
            _uow.ExamEnrollmentRepository.Update(result);
            await _uow.Commit(cancellationToken);

            return new ExamEnrollmentResponse
            {
                IsEnrolled = false,
                Status = "Created",
                /*ExamEnrollmentId = result.ExamEnrollmentId,*/
                Message = "Exam enrollment successfully created.",
                ExamEnrollment = _mapper.Map<ExamEnrollmentDto>(result)
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
                SimulationExamDetail = ee.StudentOfExams.Select(se => new ExamDetailsDto()
                {
                    ExamId = se.ExamId,
                    ExamCode = se.Exam.ExamCode,
                    ExamName = se.Exam.ExamName,
                    ExamDiscountFee = se.Exam.ExamDiscountFee,
                    ExamImage = se.Exam.ExamImage,
                    ExamFee = se.Exam.ExamFee
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
                SimulationExamDetail = result.StudentOfExams.Select(se => new ExamDetailsDto
                {
                    ExamId = se.ExamId,
                    ExamCode = se.Exam.ExamCode,
                    ExamName = se.Exam.ExamName,
                    ExamDiscountFee = se.Exam.ExamDiscountFee,
                    ExamImage = se.Exam.ExamImage,
                    ExamFee = se.Exam.ExamFee
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
                SimulationExamDetail = ee.StudentOfExams.Select(se => new ExamDetailsDto()
                {
                    ExamId = se.ExamId,
                    ExamCode = se.Exam.ExamCode,
                    ExamName = se.Exam.ExamName,
                    ExamDiscountFee = se.Exam.ExamDiscountFee,
                    ExamImage = se.Exam.ExamImage,
                    ExamFee = se.Exam.ExamFee
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
