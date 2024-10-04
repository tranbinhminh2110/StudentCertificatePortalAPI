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
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateCourseRequest> _addCourseValidator;
        private readonly IValidator<UpdateCourseRequest> _updateCourseValidator;

        public CourseService(IUnitOfWork uow, IMapper mapper,IValidator<CreateCourseRequest> addCourseValidator, IValidator<UpdateCourseRequest> updateCourseValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _addCourseValidator = addCourseValidator;
            _updateCourseValidator = updateCourseValidator;
        }

        public async Task<CourseDto> CreateCourseAsync(CreateCourseRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addCourseValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            /*            var certification = await _uow.CertificationRepository.FirstOrDefaultAsync(x => x.CertId == request.CertId, cancellationToken);

                        if (certification == null)
                        {
                            throw new Exception("Certification not found. Course creation requires a valid CertId.");
                        }*/
            var courseEntity = new Course()
            {
                CourseName = request.CourseName,
                CourseCode = request.CourseCode,
                CourseTime = request.CourseTime,
                CourseDescription = request.CourseDescription,
            };
            var result = await _uow.CourseRepository.AddAsync(courseEntity);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<CourseDto>(result);

        }

        public async Task<CourseDto> DeleteCourseAsync(int courseId, CancellationToken cancellationToken)
        {
            var course = await _uow.CourseRepository.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
            if (course is null)
            {
                throw new KeyNotFoundException("Course not found.");
            }
            _uow.CourseRepository.Delete(course);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<CourseDto>(course);
        }

        public async Task<List<CourseDto>> GetAll()
        {
            var result = await _uow.CourseRepository.GetAll();
            return _mapper.Map<List<CourseDto>>(result);
        }

        public async Task<CourseDto> GetCourseById(int courseId, CancellationToken cancellationToken)
        {
            var result = await _uow.CourseRepository.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Course not found.");
            }
            return _mapper.Map<CourseDto>(result);
        }

        public async Task<CourseDto> UpdateCourseAsync(int courseId, UpdateCourseRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateCourseValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var course = await _uow.CourseRepository.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken).ConfigureAwait(false);
            if (course is null)
            {
                throw new KeyNotFoundException("Course not found.");
            }
            course.CourseName = request.CourseName;
            course.CourseCode = request.CourseCode;
            course.CourseTime = request.CourseTime;
            course.CourseDescription = request.CourseDescription;
            _uow.CourseRepository.Update(course);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<CourseDto>(course);

        }
    }
}
