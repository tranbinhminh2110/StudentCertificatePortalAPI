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
                throw new KeyNotFoundException("User not found. Course Enrollment creation requires a valid UserId.");
            }

            if (request.Courses == null || !request.Courses.Any())
            {
                throw new ArgumentException("Courses cannot be null or empty.");
            }

            var courses = new List<Course>();
            int? totalPrice = 0;

            // Kiểm tra tất cả courseId và lưu vào danh sách courses
            foreach (var courseId in request.Courses)
            {
                var course = await _uow.CourseRepository.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
                if (course == null)
                {
                    // Nếu có khóa học không hợp lệ, ném ngoại lệ và không tạo CourseEnrollment
                    throw new KeyNotFoundException($"Course with ID {courseId} not found.");
                }

                courses.Add(course);
            }

            // Nếu đến đây, có nghĩa là tất cả courseId đều hợp lệ
            var courseEntity = new CoursesEnrollment()
            {
                UserId = request.UserId,
                CourseEnrollmentDate = DateTime.UtcNow,
                CourseEnrollmentStatus = EnumCourseEnrollment.OnGoing.ToString(),
                TotalPrice = totalPrice > 0 ? totalPrice : 0,
            };

            // Tạo CourseEnrollment
            var result = await _uow.CourseEnrollmentRepository.AddAsync(courseEntity);
            await _uow.Commit(cancellationToken);

            // Thêm các bản ghi StudentOfCourse
            foreach (var course in courses)
            {
                var studentOfCourseEntity = new StudentOfCourse()
                {
                    CreationDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddYears(1),
                    Price = course.CourseDiscountFee,
                    Status = false,
                    CourseId = course.CourseId,
                    CouseEnrollmentId = result.CourseEnrollmentId
                };

                await _uow.StudentOfCourseRepository.AddAsync(studentOfCourseEntity);
                totalPrice += course.CourseDiscountFee;
            }

            // Cập nhật tổng giá cho CourseEnrollment
            result.TotalPrice = totalPrice > 0 ? totalPrice : 0;
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

            var courseEnrollment = await _uow.CourseEnrollmentRepository.FirstOrDefaultAsync(
                x => x.CourseEnrollmentId == courseEnrollmentId,
                cancellationToken,
                include: q => q.Include(c => c.StudentOfCourses)); // Bao gồm StudentOfCourses khi lấy CourseEnrollment
            if (courseEnrollment is null)
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

            var courses = new List<Course>();
            int? totalPrice = 0;

            // Kiểm tra tất cả courseId và lưu vào danh sách courses
            foreach (var courseId in request.Courses)
            {
                var course = await _uow.CourseRepository.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
                if (course == null)
                {
                    throw new KeyNotFoundException($"Course with ID {courseId} not found.");
                }

                courses.Add(course);
            }

            // Tính tổng giá cho tất cả các khóa học hợp lệ
            foreach (var course in courses)
            {
                totalPrice += course.CourseDiscountFee;
            }

            // Cập nhật thông tin cho CourseEnrollment
            courseEnrollment.UserId = request.UserId;
            courseEnrollment.TotalPrice = totalPrice > 0 ? totalPrice : 0;
            courseEnrollment.CourseEnrollmentDate = DateTime.UtcNow;

            // **Xóa các bản ghi cũ trong StudentOfCourse**
            if (courseEnrollment.StudentOfCourses != null)
            {
                foreach (var studentOfCourse in courseEnrollment.StudentOfCourses)
                {
                    _uow.StudentOfCourseRepository.Delete(studentOfCourse);
                }
            }

            // **Thêm các bản ghi mới vào StudentOfCourse**
            foreach (var course in courses)
            {
                var studentOfCourseEntity = new StudentOfCourse()
                {
                    CreationDate = DateTime.Now,
                    ExpiryDate = DateTime.Now.AddYears(1),
                    Price = course.CourseDiscountFee,
                    Status = false,
                    CourseId = course.CourseId,
                    CouseEnrollmentId = courseEnrollment.CourseEnrollmentId
                };

                await _uow.StudentOfCourseRepository.AddAsync(studentOfCourseEntity);
            }

            // Cập nhật bản ghi trong kho dữ liệu và cam kết
            _uow.CourseEnrollmentRepository.Update(courseEnrollment);
            await _uow.Commit(cancellationToken);

            // Map và trả về kết quả đã cập nhật
            return _mapper.Map<CourseEnrollmentDto>(courseEnrollment);
        }

    }
}
