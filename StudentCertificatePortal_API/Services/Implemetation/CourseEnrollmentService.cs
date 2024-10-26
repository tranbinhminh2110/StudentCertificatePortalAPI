using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Commons;
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
            _updateCourseEnrollmentValidator = updateCourseEnrollmentValidator;
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
                throw new KeyNotFoundException("User not found. Course Enrollment creation requires a valid UserId.");
            }

            if (request.Courses == null || !request.Courses.Any())
            {
                throw new ArgumentException("Courses cannot be null or empty.");
            }

            var courses = new List<Course>();
            int? totalPrice = 0;

            foreach (var courseId in request.Courses)
            {
                var course = await _uow.CourseRepository.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
                if (course == null)
                {
                    throw new KeyNotFoundException($"Course with ID {courseId} not found.");
                }

                courses.Add(course);
            }

            var courseEntity = new CoursesEnrollment()
            {
                UserId = request.UserId,
                CourseEnrollmentDate = DateTime.UtcNow,
                CourseEnrollmentStatus = EnumCourseEnrollment.OnGoing.ToString(),
                TotalPrice = totalPrice ?? 0, 
            };

           
            var result = await _uow.CourseEnrollmentRepository.AddAsync(courseEntity);
            await _uow.Commit(cancellationToken);


            foreach (var course in courses)
            {
                var price = course.CourseDiscountFee ?? course.CourseFee;

                var studentOfCourseEntity = new StudentOfCourse()
                {
                    CreationDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddYears(1),
                    Price = price, 
                    Status = false,
                    CourseId = course.CourseId,
                    CouseEnrollmentId = result.CourseEnrollmentId
                };

                await _uow.StudentOfCourseRepository.AddAsync(studentOfCourseEntity);
                totalPrice += price; 
            }

            
            result.TotalPrice = totalPrice ?? 0; 
            _uow.CourseEnrollmentRepository.Update(result);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<CourseEnrollmentDto>(result);
        }

        public async Task<CourseEnrollmentDto> DeleteCourseEnrollmentAsync(int courseEnrollmentId, CancellationToken cancellationToken)
        {
            var courseEnrollment = await _uow.CourseEnrollmentRepository.FirstOrDefaultAsync(
                x => x.CourseEnrollmentId == courseEnrollmentId,
                cancellationToken, include: q => q.Include(c => c.StudentOfCourses).Include(c => c.Payments));

            if (courseEnrollment is null)
            {
                throw new KeyNotFoundException("Course Enrollment not found.");
            }

            courseEnrollment.StudentOfCourses?.Clear();

            _uow.CourseEnrollmentRepository.Delete(courseEnrollment);
            await _uow.Commit(cancellationToken);

            var courseEnrollmentDto = _mapper.Map<CourseEnrollmentDto>(courseEnrollment);
            return courseEnrollmentDto;
        }

        public async Task<List<CourseEnrollmentDto>> GetAll()
        {
            var result = await _uow.CourseEnrollmentRepository.GetAllAsync(
                include: q => q.Include(c => c.StudentOfCourses)
                               .ThenInclude(sc => sc.Course)); 
            var courseEnrollmentDtos = result.Select(courseEnrollment => new CourseEnrollmentDto
            {
                CourseEnrollmentId = courseEnrollment.CourseEnrollmentId,
                CourseEnrollmentDate = courseEnrollment.CourseEnrollmentDate,
                TotalPrice = courseEnrollment.TotalPrice,
                CourseEnrollmentStatus = courseEnrollment.CourseEnrollmentStatus,
                UserId = courseEnrollment.UserId,
                CourseDetails = courseEnrollment.StudentOfCourses.Select(sc => new CourseDetailsDto
                {
                    CourseId = sc.Course.CourseId,
                    CourseName = sc.Course.CourseName,
                    CourseCode = sc.Course.CourseCode,
                    CourseDiscountFee = sc.Course.CourseDiscountFee,
                    CourseImage = sc.Course.CourseImage,
                }).ToList()
            }).ToList();

            return courseEnrollmentDtos;
        }

        public async Task<CourseEnrollmentDto> GetCourseEnrollmentByIdAsync(int courseEnrollmentId, CancellationToken cancellationToken)
        {
            var result = await _uow.CourseEnrollmentRepository.FirstOrDefaultAsync(
                x => x.CourseEnrollmentId == courseEnrollmentId,
                cancellationToken,
                include: q => q.Include(c => c.StudentOfCourses)
                               .ThenInclude(sc => sc.Course)); 

            if (result is null)
            {
                throw new KeyNotFoundException("Course Enrollment not found.");
            }

            var courseEnrollmentDto = new CourseEnrollmentDto
            {
                CourseEnrollmentId = result.CourseEnrollmentId,
                CourseEnrollmentDate = result.CourseEnrollmentDate,
                TotalPrice = result.TotalPrice,
                UserId = result.UserId,
                CourseDetails = result.StudentOfCourses.Select(sc => new CourseDetailsDto
                {
                    CourseId = sc.Course.CourseId,
                    CourseName = sc.Course.CourseName,
                    CourseCode = sc.Course.CourseCode,
                    CourseDiscountFee = sc.Course.CourseDiscountFee,
                    CourseImage = sc.Course.CourseImage,
                }).ToList()
            };

            return courseEnrollmentDto;
        }

        public async Task<List<CourseEnrollmentDto>> GetCourseEnrollmentByUserIdAsync(int userId, CancellationToken cancellationToken)
        {
            var result = await _uow.CourseEnrollmentRepository.WhereAsync(
                x => x.UserId == userId,
                cancellationToken,
                include: q => q.Include(c => c.StudentOfCourses)
                               .ThenInclude(sc => sc.Course));

            if (result is null)
            {
                throw new KeyNotFoundException("Course Enrollment not found.");
            }

            var courseEnrollmentDtos = result.Select(enrollment => new CourseEnrollmentDto
            {
                CourseEnrollmentId = enrollment.CourseEnrollmentId,
                CourseEnrollmentDate = enrollment.CourseEnrollmentDate,
                TotalPrice = enrollment.TotalPrice,
                UserId = enrollment.UserId,
                CourseDetails = enrollment.StudentOfCourses.Select(sc => new CourseDetailsDto
                {
                    CourseId = sc.Course.CourseId,
                    CourseName = sc.Course.CourseName,
                    CourseCode = sc.Course.CourseCode,
                    CourseDiscountFee = sc.Course.CourseDiscountFee,
                    CourseImage = sc.Course.CourseImage,
                }).ToList()
            }).ToList();

            return courseEnrollmentDtos;
        }

        public async Task<CourseEnrollmentDto> UpdateCourseEnrollmentAsync(int courseEnrollmentId, UpdateCourseEnrollmentRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateCourseEnrollmentValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            var courseEnrollment = await _uow.CourseEnrollmentRepository.FirstOrDefaultAsync(
                x => x.CourseEnrollmentId == courseEnrollmentId,
                cancellationToken,
                include: q => q.Include(c => c.StudentOfCourses));
            if (courseEnrollment is null)
            {
                throw new KeyNotFoundException("Course Enrollment not found.");
            }

            if (request.Courses == null || !request.Courses.Any())
            {
                throw new ArgumentException("Courses cannot be null or empty.");
            }

            var courses = new List<Course>();
            int? totalPrice = 0;

            foreach (var courseId in request.Courses)
            {
                var course = await _uow.CourseRepository.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
                if (course == null)
                {
                    throw new KeyNotFoundException($"Course with ID {courseId} not found.");
                }

                courses.Add(course);
            }

            foreach (var course in courses)
            {
                totalPrice += course.CourseDiscountFee ?? course.CourseFee; 
            }

            courseEnrollment.TotalPrice = totalPrice ?? 0; 
            courseEnrollment.CourseEnrollmentDate = DateTime.UtcNow;

            if (courseEnrollment.StudentOfCourses != null)
            {
                foreach (var studentOfCourse in courseEnrollment.StudentOfCourses)
                {
                    _uow.StudentOfCourseRepository.Delete(studentOfCourse);
                }
            }

            foreach (var course in courses)
            {
                var studentOfCourseEntity = new StudentOfCourse()
                {
                    CreationDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddYears(1),
                    Price = course.CourseDiscountFee ?? course.CourseFee, 
                    Status = false,
                    CourseId = course.CourseId,
                    CouseEnrollmentId = courseEnrollment.CourseEnrollmentId
                };

                await _uow.StudentOfCourseRepository.AddAsync(studentOfCourseEntity);
            }

            _uow.CourseEnrollmentRepository.Update(courseEnrollment);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<CourseEnrollmentDto>(courseEnrollment);
        }
    }
}
