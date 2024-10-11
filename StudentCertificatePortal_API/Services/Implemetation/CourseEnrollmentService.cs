using AutoMapper;
using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class CourseEnrollmentService : ICourseEnrollmentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateCourseEnrollmentRequest> _addCourseEnrollmentValidator;
        private readonly IValidator<UpdateCourseEnrollmentRequest> _updateCourseEnrollmentValidator;

        public CourseEnrollmentService(IUnitOfWork uow, IMapper mapper, IValidator<CreateCourseEnrollmentRequest> addCourseEnrollmentValidator, IValidator<UpdateCourseEnrollmentRequest> updateCourseEnrollmentValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _addCourseEnrollmentValidator = addCourseEnrollmentValidator;
            _updateCourseEnrollmentValidator= updateCourseEnrollmentValidator;
        }

        public async Task<CourseEnrollmentDto> CreateCourseEnrollmentAsync(CreateCourseEnrollmentRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addCourseEnrollmentValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

            if (user == null)
            {
                throw new Exception("User not found!");
            }
            var courseEnrollmentEntity = new CoursesEnrollment()
            {
                CourseEnrollmentDate = request.CourseEnrollmentDate,
                CourseEnrollmentStatus = request.CourseEnrollmentStatus,
                TotalPrice = request.TotalPrice,
                UserId = request.UserId,
                
            };
            var result = await _uow.CourseEnrollmentRepository.AddAsync(courseEnrollmentEntity);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<CourseEnrollmentDto>(result);
        }

        public async Task<CourseEnrollmentDto> DeleteCourseEnrollmentAsync(int courseEnrollmentId, CancellationToken cancellationToken)
        {
            var course = await _uow.CourseEnrollmentRepository.FirstOrDefaultAsync(x => x.CourseEnrollmentId == courseEnrollmentId, cancellationToken);
            if (course is null)
            {
                throw new KeyNotFoundException("Course Enrollment not found.");
            }
            _uow.CourseEnrollmentRepository.Delete(course);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<CourseEnrollmentDto>(course);
        }

        public async Task<List<CourseEnrollmentDto>> GetAll()
        {
            var result = await _uow.CourseEnrollmentRepository.GetAll();
            return _mapper.Map<List<CourseEnrollmentDto>>(result);
        }

        public async Task<CourseEnrollmentDto> GetCourseEnrollmentByIdAsync(int courseEnrollmentId, CancellationToken cancellationToken)
        {
            var result = await _uow.CourseEnrollmentRepository.FirstOrDefaultAsync(x => x.CourseEnrollmentId == courseEnrollmentId, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Course Enrollment not found.");
            }
            return _mapper.Map<CourseEnrollmentDto>(result);
        }

        public async Task<CourseEnrollmentDto> UpdateCourseEnrollmentAsync(int courseEnrollmentId, UpdateCourseEnrollmentRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateCourseEnrollmentValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var course = await _uow.CourseEnrollmentRepository.FirstOrDefaultAsync(x => x.CourseEnrollmentId == courseEnrollmentId, cancellationToken);
            if (course is null)
            {
                throw new KeyNotFoundException("Course Enrollment not found.");
            }
            course.CourseEnrollmentDate = request.CourseEnrollmentDate;
            course.CourseEnrollmentStatus = request.CourseEnrollmentStatus;
            course.TotalPrice = request.TotalPrice;
            _uow.CourseEnrollmentRepository.Update(course);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<CourseEnrollmentDto>(course);
        }
    }
}
