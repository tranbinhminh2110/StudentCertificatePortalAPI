using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
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
        public async Task<ExamEnrollmentDto> CreateExamEnrollmentAsync(CreateExamEnrollmentRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addExamEnrollmentValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);
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
                if (simulation != null)
                {
                    simulations.Add(simulation);
                }
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

            return _mapper.Map<ExamEnrollmentDto>(result);
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
            var result = await _uow.ExamEnrollmentRepository.GetAll();
            return _mapper.Map<List<ExamEnrollmentDto>>(result);
        }

        public async Task<ExamEnrollmentDto> GetExamEnrollmentById(int examEnrollmentId, CancellationToken cancellationToken)
        {
            var result = await _uow.ExamEnrollmentRepository.FirstOrDefaultAsync(x => x.ExamEnrollmentId == examEnrollmentId, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Exam Enrollment not found.");
            }
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


    }
}
