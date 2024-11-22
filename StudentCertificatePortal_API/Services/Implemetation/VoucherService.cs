using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class VoucherService : IVoucherService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IValidator<CreateVoucherRequest> _addVoucherValidator;
        private readonly IValidator<UpdateVoucherRequest> _updateVoucherValidator;

        public VoucherService(IUnitOfWork uow, IMapper mapper,
            IValidator<CreateVoucherRequest> addVoucherValidator, IValidator<UpdateVoucherRequest> updateVoucherValidator
            )
        {
            _uow = uow;
            _mapper = mapper;
            _addVoucherValidator = addVoucherValidator;
            _updateVoucherValidator = updateVoucherValidator;
        }

        public async Task<VoucherDto> CreateVoucherAsync(CreateVoucherRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addVoucherValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var voucherEntity = new Voucher()
            {
                VoucherName = request.VoucherName,
                VoucherDescription = request.VoucherDescription,
                Percentage = request.Percentage,
                CreationDate = request.CreationDate,
                ExpiryDate = request.ExpiryDate,
                VoucherStatus = request.ExpiryDate > DateTime.Now 
            };
            if (request.ExamId != null && request.ExamId.Any())
            {
                foreach (var examId in request.ExamId)
                {
                    var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(
                        x => x.ExamId == examId, cancellationToken);
                    if (exam != null)
                    {
                        voucherEntity.Exams.Add(exam);
                    }
                    else
                    {
                        throw new KeyNotFoundException($"SimulationExam with ID {examId} not found.");
                    }
                }

            }
            if (request.CourseId != null && request.CourseId.Any())
            {
                foreach (var courseId in request.CourseId)
                {
                    var course = await _uow.CourseRepository.FirstOrDefaultAsync(
                        x => x.CourseId == courseId, cancellationToken);
                    if (course != null)
                    {
                        voucherEntity.Courses.Add(course);
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Course with ID {courseId} not found.");
                    }
                }

            }
            float discountRate = 1 - (voucherEntity.Percentage ?? 0) / 100f;

            foreach (var course in voucherEntity.Courses)
            {
                if (course.CourseFee.HasValue)
                {
                    course.CourseDiscountFee = (int?)(course.CourseFee.Value * discountRate);
                }
            }

            foreach (var exam in voucherEntity.Exams)
            {
                if (exam.ExamFee.HasValue)
                {
                    exam.ExamDiscountFee = (int?)(exam.ExamFee.Value * discountRate);
                }
            }

            await _uow.VoucherRepository.AddAsync(voucherEntity);
            try
            {
                await _uow.Commit(cancellationToken);
                var voucherDto = _mapper.Map<VoucherDto>(voucherEntity);
                voucherDto.ExamId = voucherEntity.Exams
                    .Select(voucherr => voucherr.ExamId)
                    .ToList();                
                voucherDto.CourseId = voucherEntity.Courses
                    .Select(voucherr => voucherr.CourseId)
                    .ToList();
                return voucherDto;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? "No inner exception";
                throw new Exception($"An error occurred while saving the entity changes: {innerExceptionMessage}", ex);

            }
        }

        public async Task<VoucherDto> DeleteVoucherAsync(int voucherId, CancellationToken cancellationToken)
        {
            var voucher = await _uow.VoucherRepository.FirstOrDefaultAsync(
                x => x.VoucherId == voucherId,
                cancellationToken, include: q => q.Include(c => c.Exams)
                .Include(c => c.Courses));
            if (voucher is null) {
                throw new KeyNotFoundException("Voucher not found.");
            }
            if (voucher.Courses != null && voucher.Courses.Any())
            {
                foreach (var course in voucher.Courses)
                {
                    course.CourseDiscountFee = course.CourseFee; 
                }
            }
            if (voucher.Exams != null && voucher.Exams.Any())
            {
                foreach (var exam in voucher.Exams)
                {
                    exam.ExamDiscountFee = exam.ExamFee;
                }
            }
            voucher.Exams?.Clear();
            voucher.Courses?.Clear();

            _uow.VoucherRepository.Delete(voucher);
            await _uow.Commit(cancellationToken);

            var voucherDto = _mapper.Map<VoucherDto>(voucher);
            return voucherDto;
        }

        public async Task<List<VoucherDto>> GetAll(CancellationToken cancellationToken)
        {
            var expiredVouchers = await _uow.VoucherRepository.WhereAsync(v =>
                v.ExpiryDate <= DateTime.Now && v.VoucherStatus == true);

            foreach (var voucher in expiredVouchers)
            {
                voucher.VoucherStatus = false;
                _uow.VoucherRepository.Update(voucher);
            }

            var validVouchers = await _uow.VoucherRepository.WhereAsync(v =>
                v.ExpiryDate > DateTime.Now && v.VoucherStatus == false);

            foreach (var voucher in validVouchers)
            {
                voucher.VoucherStatus = true;
                _uow.VoucherRepository.Update(voucher);
            }

            await _uow.Commit(cancellationToken);

            var result = await _uow.VoucherRepository.GetAllAsync(query =>
            query.Include(c => c.Exams)
            .Include(c => c.Courses));

            var voucherDtos = result.Select(result =>
            {
                var voucherDto = _mapper.Map<VoucherDto>(result);

                voucherDto.ExamDetails = result.Exams
                    .Select(exam => new ExamDetailsDto
                    {
                        ExamId = exam.ExamId,
                        ExamName = exam.ExamName,
                        ExamCode = exam.ExamCode,
                        ExamFee = exam.ExamFee,
                        ExamDiscountFee = exam.ExamDiscountFee,
                        ExamImage = exam.ExamImage,
                    }).ToList();
                voucherDto.CourseDetails = result.Courses
                    .Select(course => new CourseDetailsDto
                    {
                        CourseId = course.CourseId,
                        CourseName = course.CourseName,
                        CourseCode = course.CourseCode,
                        CourseTime = course.CourseTime,
                        CourseFee = course.CourseFee,
                        CourseDiscountFee = course.CourseDiscountFee,
                        CourseImage = course.CourseImage,
              
                    }).ToList();
                return voucherDto;
            }).ToList();
            return voucherDtos;
        }

        public async Task<VoucherDto> GetVoucherByIdAsync(int voucherId, CancellationToken cancellationToken)
        {
            var expiredVouchers = await _uow.VoucherRepository.WhereAsync(v =>
                v.ExpiryDate <= DateTime.Now && v.VoucherStatus == true);

            foreach (var voucher in expiredVouchers)
            {
                voucher.VoucherStatus = false;
                _uow.VoucherRepository.Update(voucher);
            }

            var validVouchers = await _uow.VoucherRepository.WhereAsync(v =>
                v.ExpiryDate > DateTime.Now && v.VoucherStatus == false);

            foreach (var voucher in validVouchers)
            {
                voucher.VoucherStatus = true;
                _uow.VoucherRepository.Update(voucher);
            }

            await _uow.Commit(cancellationToken);
            var result = await _uow.VoucherRepository.FirstOrDefaultAsync(
                x => x.VoucherId == voucherId, cancellationToken: cancellationToken, include: query => query.Include(c => c.Exams)
                .Include(c => c.Courses));
            if (result is null)
            {
                throw new KeyNotFoundException("Voucher not founc.");
            }
            var voucherDto = _mapper.Map<VoucherDto>(result);
            voucherDto.ExamDetails = result.Exams
                    .Select(exam => new ExamDetailsDto
                    {
                        ExamId = exam.ExamId,
                        ExamName = exam.ExamName,
                        ExamCode = exam.ExamCode,
                        ExamFee = exam.ExamFee,
                        ExamDiscountFee = exam.ExamDiscountFee,
                        ExamImage = exam.ExamImage,
                    }).ToList();
            voucherDto.CourseDetails = result.Courses
                .Select(course => new CourseDetailsDto
                {
                    CourseId = course.CourseId,
                    CourseName = course.CourseName,
                    CourseCode = course.CourseCode,
                    CourseTime = course.CourseTime,
                    CourseFee = course.CourseFee,
                    CourseDiscountFee = course.CourseDiscountFee,
                    CourseImage = course.CourseImage,

                }).ToList();
            return voucherDto;
        }

        public async Task<List<VoucherDto>> GetVoucherByNameAsync(string voucherName, CancellationToken cancellationToken)
        {
            var expiredVouchers = await _uow.VoucherRepository.WhereAsync(v =>
                v.ExpiryDate <= DateTime.Now && v.VoucherStatus == true);

            foreach (var voucher in expiredVouchers)
            {
                voucher.VoucherStatus = false;
                _uow.VoucherRepository.Update(voucher);
            }

            var validVouchers = await _uow.VoucherRepository.WhereAsync(v =>
                v.ExpiryDate > DateTime.Now && v.VoucherStatus == false);

            foreach (var voucher in validVouchers)
            {
                voucher.VoucherStatus = true;
                _uow.VoucherRepository.Update(voucher);
            }

            await _uow.Commit(cancellationToken);
            var result = await _uow.VoucherRepository.WhereAsync(x => x.VoucherName.Contains(voucherName), cancellationToken,
                include: query => query.Include(c => c.Exams)
                .Include(c => c.Courses));
            if (result is null || !result.Any())
            {
                throw new KeyNotFoundException("Voucher not found.");
            }
            var voucherDtos = _mapper.Map<List<VoucherDto>>(result);
            foreach (var voucherDto in voucherDtos)
            {
                var voucher = result.FirstOrDefault(x => x.VoucherId == voucherDto.VoucherId);
                voucherDto.ExamDetails = voucher.Exams
                    .Select(exam => new ExamDetailsDto
                    {
                        ExamId = exam.ExamId,
                        ExamName = exam.ExamName,
                        ExamCode = exam.ExamCode,
                        ExamFee = exam.ExamFee,
                        ExamDiscountFee = exam.ExamDiscountFee,
                        ExamImage = exam.ExamImage,
                    }).ToList();
                voucherDto.CourseDetails = voucher.Courses
                    .Select(course => new CourseDetailsDto
                    {
                        CourseId = course.CourseId,
                        CourseName = course.CourseName,
                        CourseCode = course.CourseCode,
                        CourseTime = course.CourseTime,
                        CourseFee = course.CourseFee,
                        CourseDiscountFee = course.CourseDiscountFee,
                        CourseImage = course.CourseImage,
                    }).ToList();
            }
            return voucherDtos;
        }

        public async Task<VoucherDto> UpdateVoucherAsync(int voucherId, UpdateVoucherRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateVoucherValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            // Find the voucher by ID
            var voucher = await _uow.VoucherRepository
                .Include(x => x.Exams)
                .Include(x => x.Courses)
                .FirstOrDefaultAsync(x => x.VoucherId == voucherId, cancellationToken);

            if (voucher == null)
            {
                throw new KeyNotFoundException("Voucher not found.");
            }

            // Update voucher properties
            voucher.VoucherName = request.VoucherName;
            voucher.VoucherDescription = request.VoucherDescription;
            voucher.Percentage = request.Percentage;
            voucher.CreationDate = request.CreationDate;
            voucher.ExpiryDate = request.ExpiryDate;
            voucher.VoucherStatus = request.ExpiryDate > DateTime.Now;


            // Get existing Exam IDs
            var existingExamIds = voucher.Exams.Select(e => e.ExamId).ToList();
            var newExamIds = request.ExamId ?? new List<int>();

            // Remove Exams that are no longer referenced
            foreach (var existingExamId in existingExamIds)
            {
                if (!newExamIds.Contains(existingExamId))
                {
                    var examToRemove = voucher.Exams.FirstOrDefault(e => e.ExamId == existingExamId);
                    if (examToRemove != null)
                    {
                        voucher.Exams.Remove(examToRemove);
                    }
                }
            }

            // Add new Exams that are not already in the Voucher
            foreach (var newExamId in newExamIds)
            {
                if (!existingExamIds.Contains(newExamId))
                {
                    var exam = await _uow.SimulationExamRepository
                        .FirstOrDefaultAsync(x => x.ExamId == newExamId, cancellationToken);

                    if (exam != null)
                    {
                        // Check if this relationship already exists to avoid duplicates
                        if (!voucher.Exams.Any(e => e.ExamId == newExamId))
                        {
                            voucher.Exams.Add(exam);
                        }
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Exam with ID {newExamId} not found.");
                    }
                }
            }
            var existingCourseIds = voucher.Courses.Select(e => e.CourseId).ToList();
            var newCourseIds = request.CourseId ?? new List<int>();

            // Remove Exams that are no longer referenced
            foreach (var existingCourseId in existingCourseIds)
            {
                if (!newCourseIds.Contains(existingCourseId))
                {
                    var courseToRemove = voucher.Courses.FirstOrDefault(e => e.CourseId == existingCourseId);
                    if (courseToRemove != null)
                    {
                        voucher.Courses.Remove(courseToRemove);
                    }
                }
            }

            // Add new Exams that are not already in the Voucher
            foreach (var newCourseId in newCourseIds)
            {
                if (!existingCourseIds.Contains(newCourseId))
                {
                    var course = await _uow.CourseRepository
                        .FirstOrDefaultAsync(x => x.CourseId == newCourseId, cancellationToken);

                    if (course != null)
                    {
                        // Check if this relationship already exists to avoid duplicates
                        if (!voucher.Courses.Any(e => e.CourseId == newCourseId))
                        {
                            voucher.Courses.Add(course);
                        }
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Course with ID {newCourseId} not found.");
                    }
                }
            }


            // Update the Voucher in the repository
            float discountRate = 1 - (voucher.Percentage ?? 0) / 100f;

            foreach (var course in voucher.Courses)
            {
                if (course.CourseFee.HasValue)
                {
                    course.CourseDiscountFee = (int?)(course.CourseFee.Value * discountRate);
                }
            }
            foreach (var exam in voucher.Exams)
            {
                if (exam.ExamFee.HasValue)
                {
                    exam.ExamDiscountFee = (int?)(exam.ExamFee.Value * discountRate);
                }
            }


            _uow.VoucherRepository.Update(voucher);

            try
            {
                await _uow.Commit(cancellationToken);

                // Create the DTO and populate Exam details
                var voucherDto = _mapper.Map<VoucherDto>(voucher);
                voucherDto.ExamId = voucher.Exams.Select(e => e.ExamId).ToList();
                voucherDto.CourseId = voucher.Courses.Select(e => e.CourseId).ToList();

                return voucherDto;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("The Voucher you're trying to update has been modified by another user. Please try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? "No inner exception";
                throw new Exception($"An error occurred while saving the entity changes: {innerExceptionMessage}", ex);
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? "No inner exception";
                throw new Exception($"An unexpected error occurred: {innerExceptionMessage}", ex);
            }
        }
    }
}
