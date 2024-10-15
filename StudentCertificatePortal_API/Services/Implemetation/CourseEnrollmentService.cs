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
                throw new KeyNotFoundException("User not found. Couse Enrollment creation requires a valid UserId.");
            }

            if (request.Courses == null || !request.Courses.Any())
            {
                throw new ArgumentException("Course cannot be null or empty.");
            }

            int? totalPrice = 0;

            var courses = new List<Course>();

            foreach (var courseId in request.Courses)
            {
                var course = await _uow.CourseRepository.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
                if (course != null)
                {
                    courses.Add(course);
                }
            }


            foreach (var course in courses)
            {
                if (course == null)
                {
                    throw new KeyNotFoundException($"Course with ID {course.CourseId} not found.");
                }

                totalPrice += course.CourseFee * (1 - course.CourseDiscountFee);
            }

            var courseEntity = new CoursesEnrollment()
            {
                UserId = request.UserId,
                CourseEnrollmentDate = DateTime.UtcNow,
                CourseEnrollmentStatus = EnumCourseEnrollment.OnGoing.ToString(),
                TotalPrice = totalPrice > 0? totalPrice : 0,
            };

            var result = await _uow.CourseEnrollmentRepository.AddAsync(courseEntity);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<CourseEnrollmentDto>(result);
        }

        public async Task<CourseEnrollmentDto> DeleteCourseEnrollmentAsync(int courseEnrollmentId, CancellationToken cancellationToken)
        {
            var course = await _uow.CourseEnrollmentRepository.FirstOrDefaultAsync(x => x.CourseEnrollmentId == courseEnrollmentId, cancellationToken,
                include: p => p.Include(q => q.StudentOfCourses)
                               .Include(q => q.Payments));
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
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found. Course Enrollment update requires a valid UserId.");
            }

            if (request.Courses == null || !request.Courses.Any())
            {
                throw new ArgumentException("Courses cannot be null or empty.");
            }

            int? totalPrice = 0;

            var courses = new List<Course>();
            foreach (var courseId in request.Courses)
            {
                var coursee = await _uow.CourseRepository.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
                if (coursee != null)
                {
                    courses.Add(coursee);
                }
            }

            foreach (var coursee in courses)
            {
                if (coursee == null)
                {
                    throw new KeyNotFoundException($"Course with ID {coursee.CourseId} not found.");
                }

                totalPrice += coursee.CourseFee * (1 - coursee.CourseDiscountFee / 100);
            }

            course.UserId = request.UserId;
            course.TotalPrice = totalPrice > 0 ? totalPrice : 0;
            course.CourseEnrollmentDate = DateTime.UtcNow; // Update to the current time, or use a custom field in the request if needed

            // Update the record in the repository and commit
            _uow.CourseEnrollmentRepository.Update(course);
            await _uow.Commit(cancellationToken);

            // Map and return the updated result
            return _mapper.Map<CourseEnrollmentDto>(course);
        }
    }
}
