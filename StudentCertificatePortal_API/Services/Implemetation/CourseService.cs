using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class CourseService : ICourseService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IVoucherService _voucherService;

        private readonly IValidator<CreateCourseRequest> _addCourseValidator;
        private readonly IValidator<UpdateCourseRequest> _updateCourseValidator;

        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;
        public CourseService(IUnitOfWork uow, IMapper mapper,IValidator<CreateCourseRequest> addCourseValidator
            , IValidator<UpdateCourseRequest> updateCourseValidator, IVoucherService voucherService
            , IHubContext<NotificationHub> hubContext, INotificationService notificationService)
        {
            _uow = uow;
            _mapper = mapper;
            _addCourseValidator = addCourseValidator;
            _updateCourseValidator = updateCourseValidator;
            _voucherService = voucherService;
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        public async Task<CourseDto> CreateCourseAsync(CreateCourseRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addCourseValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var certification = await _uow.CertificationRepository.FirstOrDefaultAsync(x => x.CertId == request.CertId, cancellationToken);

            if (certification == null)
            {
                throw new Exception("Certification not found. Course creation requires a valid CertId.");
            }
            var courseEntity = new Course()
            {
                CourseName = request.CourseName,
                CourseCode = request.CourseCode,
                CourseTime = request.CourseTime,
                CourseDescription = request.CourseDescription,
                CourseFee = request.CourseFee,
                CourseImage = request.CourseImage,
                CertId = request.CertId,
                CoursePermission = "Pending",
            };
            var result = await _uow.CourseRepository.AddAsync(courseEntity);
            await _uow.Commit(cancellationToken);

            float? totalDiscount = 1;

            foreach (var voucherId in request.VoucherIds)
            {
                if (voucherId > 0)
                {
                    var voucher = await _voucherService.GetVoucherByIdAsync(voucherId, cancellationToken);
                    if (voucher is null || voucher.VoucherStatus == false)
                    {
                        throw new KeyNotFoundException("Voucher not found or is expired.");
                    }

                    var existingVoucher = await _uow.VoucherRepository.FirstOrDefaultAsync(v => v.VoucherId == voucherId, cancellationToken);

                    if (existingVoucher != null)
                    {
                        result.Vouchers.Add(existingVoucher);
                    }
                    else
                    {
                        result.Vouchers.Add(_mapper.Map<Voucher>(voucher));
                    }

                    totalDiscount = totalDiscount * (1 - voucher.Percentage / 100f);
                }
            }

            result.CourseDiscountFee = (int?)(result.CourseFee.Value * totalDiscount);

            _uow.CourseRepository.Update(result);
            await _uow.Commit(cancellationToken);
            var notification = new Notification()
            {
                NotificationName = "New Course Created",
                NotificationDescription = $"A new course '{request.CourseName}' has been created and is pending approval.",
                NotificationImage = request.CourseImage,
                CreationDate = DateTime.UtcNow,
                Role = "Manager",
                IsRead = false,
                NotificationType = "Course",
                NotificationTypeId = result.CourseId,
            };
            await _uow.NotificationRepository.AddAsync(notification);



            await _uow.Commit(cancellationToken);

            var notifications = await _notificationService.GetNotificationByRoleAsync("manager", new CancellationToken());
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);
            return _mapper.Map<CourseDto>(result);

        }

        public async Task<CourseDto> DeleteCourseAsync(int courseId, CancellationToken cancellationToken)
        {
            var course = await _uow.CourseRepository.FirstOrDefaultAsync(
                x => x.CourseId == courseId,
                cancellationToken, include: q => q.Include(c => c.Vouchers)
                .Include(c => c.Carts)
                .Include(c => c.StudentOfCourses));

            if (course is null)
            {
                throw new KeyNotFoundException("Course not found.");
            }

            // Clear related entities before deleting
            course.Carts?.Clear();
            course.Vouchers?.Clear();
            course.StudentOfCourses?.Clear();

            _uow.CourseRepository.Delete(course);
            await _uow.Commit(cancellationToken);

            var courseDto = _mapper.Map<CourseDto>(course);
            return courseDto;
        }

        public async Task<List<CourseDto>> GetAll()
        {
            var result = await _uow.CourseRepository.GetAllAsync(query =>
                query.Include(c => c.Vouchers)
                     .Include(c => c.Cert)
                     .ThenInclude(cert => cert.Type));

            var courseDtos = result.Select(course =>
            {
                var courseDto = _mapper.Map<CourseDto>(course);

                courseDto.CertificationDetails = course.Cert != null ? new List<CertificationDetailsDto>
            {
            new CertificationDetailsDto
            {
                CertId = course.Cert.CertId,
                CertName = course.Cert.CertName,
                CertCode = course.Cert.CertCode,
                CertDescription = course.Cert.CertDescription,
                CertImage = course.Cert.CertImage,
                TypeName = course.Cert.Type?.TypeName,
                Permission = course.Cert.Permission,
            }
            } : new List<CertificationDetailsDto>();
                courseDto.VoucherDetails = course.Vouchers != null ? course.Vouchers.Select(voucher => new VoucherDetailsDto
                {
                    VoucherId = voucher.VoucherId,
                    VoucherName = voucher.VoucherName,
                    VoucherDescription = voucher.VoucherDescription,
                    Percentage = voucher.Percentage,
                    CreationDate = voucher.CreationDate,
                    ExpiryDate = voucher.ExpiryDate,
                    VoucherStatus = voucher.VoucherStatus
                }).ToList() : new List<VoucherDetailsDto>();

                return courseDto;
            }).ToList();

            return courseDtos;
        }
        public async Task<CourseDto> GetCourseByIdAsync(int courseId, CancellationToken cancellationToken)
        {
            var result = await _uow.CourseRepository.FirstOrDefaultAsync(
                x => x.CourseId == courseId,
                cancellationToken,
                include: query => query.Include(c => c.Vouchers)
                                      .Include(c => c.Cert)
                                      .ThenInclude(cert => cert.Type));

            if (result is null)
            {
                throw new KeyNotFoundException("Course not found.");
            }

            var courseDto = _mapper.Map<CourseDto>(result);

            courseDto.CertificationDetails = result.Cert != null ? new List<CertificationDetailsDto>
    {
        new CertificationDetailsDto
        {
            CertId = result.Cert.CertId,
            CertName = result.Cert.CertName,
            CertCode = result.Cert.CertCode,
            CertDescription = result.Cert.CertDescription,
            CertImage = result.Cert.CertImage,
            TypeName = result.Cert.Type?.TypeName,
            Permission = result.Cert.Permission,
        }
    } : new List<CertificationDetailsDto>();

            courseDto.VoucherDetails = result.Vouchers != null ? result.Vouchers.Select(voucher => new VoucherDetailsDto
            {
                VoucherId = voucher.VoucherId,
                VoucherName = voucher.VoucherName,
                VoucherDescription = voucher.VoucherDescription,
                Percentage = voucher.Percentage,
                CreationDate = voucher.CreationDate,
                ExpiryDate = voucher.ExpiryDate,
                VoucherStatus = voucher.VoucherStatus
            }).ToList() : new List<VoucherDetailsDto>();

            return courseDto;
        }


        public async Task<List<CourseDto>> GetCourseByNameAsync(string courseName, CancellationToken cancellationToken)
        {
            var result = await _uow.CourseRepository.WhereAsync(
    x => x.CourseName.Contains(courseName),
    cancellationToken,
    include: query => query.Include(c => c.Vouchers)
                           .Include(c => c.Cert)
                           .ThenInclude(cert => cert.Type));

            if (result is null || !result.Any())
            {
                throw new KeyNotFoundException("Course not found.");
            }

            var courseDtos = result.Select(course =>
            {
                var courseDto = _mapper.Map<CourseDto>(course);

                courseDto.CertificationDetails = course.Cert != null ? new List<CertificationDetailsDto>
        {
            new CertificationDetailsDto
            {
                CertId = course.Cert.CertId,
                CertName = course.Cert.CertName,
                CertCode = course.Cert.CertCode,
                CertDescription = course.Cert.CertDescription,
                CertImage = course.Cert.CertImage,
                TypeName = course.Cert.Type?.TypeName,
                Permission = course.Cert.Permission,
            }
        } : new List<CertificationDetailsDto>();

                courseDto.VoucherDetails = course.Vouchers != null ? course.Vouchers.Select(voucher => new VoucherDetailsDto
                {
                    VoucherId = voucher.VoucherId,
                    VoucherName = voucher.VoucherName,
                    VoucherDescription = voucher.VoucherDescription,
                    Percentage = voucher.Percentage,
                    CreationDate = voucher.CreationDate,
                    ExpiryDate = voucher.ExpiryDate,
                    VoucherStatus = voucher.VoucherStatus
                }).ToList() : new List<VoucherDetailsDto>();

                return courseDto;
            }).ToList();

            return courseDtos;
        }

        public async Task<CourseDto> UpdateCourseAsync(int courseId, UpdateCourseRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateCourseValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var course = await _uow.CourseRepository.FirstOrDefaultAsync(x => x.CourseId == courseId
            , cancellationToken
            , include: x => x.Include(p => p.Vouchers));
            if (course is null)
            {
                throw new KeyNotFoundException("Course not found.");
            }
            course.CourseName = request.CourseName;
            course.CourseCode = request.CourseCode;
            course.CourseTime = request.CourseTime;
            course.CourseDescription = request.CourseDescription;
            course.CourseFee = request.CourseFee;
            course.CourseImage = request.CourseImage;
            course.CertId = request.CertId;
            course.CoursePermission = "Pending";


            float? totalDiscount = 1;

            course.Vouchers.Clear();

            foreach (var voucherId in request.VoucherIds)
            {
                if (voucherId > 0)
                {
                    var voucher = await _voucherService.GetVoucherByIdAsync(voucherId, cancellationToken);
                    if (voucher == null || voucher.VoucherStatus == false)
                    {
                        throw new KeyNotFoundException("Voucher not found or is expired.");
                    }

                    var existingVoucher = await _uow.VoucherRepository.FirstOrDefaultAsync(v => v.VoucherId == voucherId, cancellationToken);
                    if (existingVoucher != null)
                    {
                        course.Vouchers.Add(existingVoucher);
                    }
                    else
                    {
                        course.Vouchers.Add(_mapper.Map<Voucher>(voucher));
                    }

                    totalDiscount *= (1 - voucher.Percentage / 100f);
                }
            }

            if (course.CourseFee.HasValue)
            {
                course.CourseDiscountFee = (int?)(course.CourseFee.Value * totalDiscount);
            }

            _uow.CourseRepository.Update(course);
            // Create and add notification for certification update
            var notification = new Notification
            {
                NotificationName = "Course Updated",
                NotificationDescription = $"The certification '{course.CourseName}' has been updated and is pending approval.",
                NotificationImage = request.CourseImage,
                CreationDate = DateTime.UtcNow,
                Role = "Manager",
                IsRead = false,
                NotificationType = "Course",
                NotificationTypeId = course.CourseId,

            };
            await _uow.NotificationRepository.AddAsync(notification);

            // Commit changes to database
            await _uow.Commit(cancellationToken);

            var notifications = await _notificationService.GetNotificationByRoleAsync("manager", new CancellationToken());
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);

            return _mapper.Map<CourseDto>(course);

        }

        public async Task<CourseDto> UpdateCoursePermissionAsync(int courseId, EnumPermission coursePermission, CancellationToken cancellationToken)
        {
            var course = await _uow.CourseRepository.FirstOrDefaultAsync(x => x.CourseId == courseId, cancellationToken);

            if (course is null)
            {
                throw new KeyNotFoundException("Course not found.");
            }

            course.CoursePermission = coursePermission.ToString();

            _uow.CourseRepository.Update(course);
            await _uow.Commit(cancellationToken);
            var notification = new Notification
            {
                NotificationName = "Course Permission Update",
                NotificationDescription = $"The course '{course.CourseName}' has been {coursePermission}.",
                NotificationImage = course.CourseImage,
                CreationDate = DateTime.UtcNow,
                Role = "Staff",
                IsRead = false,
                NotificationType = "Course",
                NotificationTypeId = course.CourseId,

            };

            await _uow.NotificationRepository.AddAsync(notification);
            await _uow.Commit(cancellationToken);


            var notifications = await _notificationService.GetNotificationByRoleAsync("staff", new CancellationToken());
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);
            var result = _mapper.Map<CourseDto>(course);

            return result;
        }
        public async Task<CourseDto> UpdateCourseVoucherAsync(int courseId, List<int> voucherIds, CancellationToken cancellationToken)
        {
            // Lấy thông tin course từ database
            var course = await _uow.CourseRepository.FirstOrDefaultAsync(
                x => x.CourseId == courseId,
                cancellationToken,
                include: query => query.Include(c => c.Vouchers)
            );

            if (course == null)
            {
                throw new KeyNotFoundException("Course not found.");
            }

            // Xóa các voucher hiện tại
            course.Vouchers.Clear();

            float? totalDiscount = 1;

            foreach (var voucherId in voucherIds)
            {
                if (voucherId > 0)
                {
                    // Lấy thông tin voucher
                    var voucher = await _voucherService.GetVoucherByIdAsync(voucherId, cancellationToken);

                    if (voucher == null || voucher.VoucherStatus == false)
                    {
                        throw new KeyNotFoundException($"Voucher with ID {voucherId} not found or is expired.");
                    }

                    var existingVoucher = await _uow.VoucherRepository.FirstOrDefaultAsync(
                        v => v.VoucherId == voucherId,
                        cancellationToken
                    );

                    if (existingVoucher != null)
                    {
                        course.Vouchers.Add(existingVoucher);
                    }
                    else
                    {
                        course.Vouchers.Add(_mapper.Map<Voucher>(voucher));
                    }

                    totalDiscount *= (1 - voucher.Percentage / 100f);
                }
            }

            // Tính lại giá giảm
            if (course.CourseFee.HasValue)
            {
                course.CourseDiscountFee = (int?)(course.CourseFee.Value * totalDiscount);
            }

            // Cập nhật course
            _uow.CourseRepository.Update(course);
            await _uow.Commit(cancellationToken);

            // Trả về DTO sau khi cập nhật
            var courseDto = _mapper.Map<CourseDto>(course);
            courseDto.VoucherDetails = course.Vouchers.Select(v => new VoucherDetailsDto
            {
                VoucherId = v.VoucherId,
                VoucherName = v.VoucherName,
                VoucherDescription = v.VoucherDescription,
                Percentage = v.Percentage,
                CreationDate = v.CreationDate,
                ExpiryDate = v.ExpiryDate,
                VoucherStatus = v.VoucherStatus
            }).ToList();

            return courseDto;
        }


    }
}
