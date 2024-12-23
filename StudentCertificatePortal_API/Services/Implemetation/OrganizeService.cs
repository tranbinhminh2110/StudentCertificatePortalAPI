using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
    public class OrganizeService : IOrganizeService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateOrganizeRequest> _addOrganizeValidator;
        private readonly IValidator<UpdateOrganizeRequest> _updateOrganizeValidator;

        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;
        public OrganizeService(IUnitOfWork uow, IMapper mapper, IValidator<CreateOrganizeRequest> addOrganizeValidator, IValidator<UpdateOrganizeRequest> updateOrganizeValidator
            , IHubContext<NotificationHub> hubContext, INotificationService notificationService)
        {
            _uow = uow;
            _mapper = mapper;
            _addOrganizeValidator = addOrganizeValidator;
            _updateOrganizeValidator = updateOrganizeValidator;
            _hubContext = hubContext;
            _notificationService = notificationService;
        }
        public async Task<OrganizeDto> CreateOrganizeAsync(CreateOrganizeRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addOrganizeValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var organizeEntity = new Organize()
            {
                OrganizeName = request.OrganizeName,
                OrganizeAddress = request.OrganizeAddress?.Trim(),
                OrganizeContact = request.OrganizeContact,
                OrganizePermission = "Pending",
            };
            var result = await _uow.OrganizeRepository.AddAsync(organizeEntity);
            await _uow.Commit(cancellationToken);
            var notification = new Notification()
            {
                NotificationName = "New Organize Created",
                NotificationDescription = $"A new organization '{request.OrganizeName}' has been created and is pending approval.",
                NotificationImage = null, 
                CreationDate = DateTime.UtcNow,
                Role = "manager",
                IsRead = false,
                NotificationType = "organizations",
                NotificationTypeId = organizeEntity.OrganizeId,

            };
            await _uow.NotificationRepository.AddAsync(notification);
            await _uow.Commit(cancellationToken);

            var notifications = await _notificationService.GetNotificationByRoleAsync("manager", new CancellationToken());
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);
            return _mapper.Map<OrganizeDto>(result);
        }

        public async Task<OrganizeDto> DeleteOrganizeAsync(int organizeId, CancellationToken cancellationToken)
        {
            var organize = await _uow.OrganizeRepository.FirstOrDefaultAsync(x => x.OrganizeId == organizeId, cancellationToken,
                include: x => x.Include(c => c.Certifications)); 
            if (organize is null)
            {
                throw new KeyNotFoundException("Organize not found.");
            }
            organize.Certifications?.Clear();

            _uow.OrganizeRepository.Delete(organize);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<OrganizeDto>(organize);
        }

        public async Task<List<OrganizeDto>> GetAll()
        {
            var result = await _uow.OrganizeRepository.GetAllAsync(
                include: x => x.Include(o => o.Certifications)
                            .ThenInclude(cert => cert.Type)
            );

            var organizeDtos = result.Select(organize => new OrganizeDto
            {
                OrganizeId = organize.OrganizeId,
                OrganizeName = organize.OrganizeName,
                OrganizeAddress = organize.OrganizeAddress,
                OrganizeContact = organize.OrganizeContact,
                OrganizePermission = organize.OrganizePermission,
                CertificationDetails = organize.Certifications.Select(cert => new CertificationDetailsDto
                {
                    CertId = cert.CertId,
                    CertName = cert.CertName,
                    CertCode = cert.CertCode,
                    CertDescription = cert.CertDescription,
                    CertImage = cert.CertImage,
                    TypeName = cert.Type?.TypeName,
                    CertValidity = cert.CertValidity,
                    OrganizeName = cert.Organize?.OrganizeName,
                    Permission = cert.Permission,
                }).ToList()
            }).ToList();

            return organizeDtos;
        }

        public async Task<OrganizeDto> GetOrganizeByIdAsync(int organizeId, CancellationToken cancellationToken)
        {
            var organize = await _uow.OrganizeRepository.FirstOrDefaultAsync(
                x => x.OrganizeId == organizeId,
                cancellationToken,
                include: x => x.Include(o => o.Certifications)
                            .ThenInclude(cert => cert.Type)

            );

            if (organize is null)
            {
                throw new KeyNotFoundException("Organize not found.");
            }

            var organizeDto = new OrganizeDto
            {
                OrganizeId = organize.OrganizeId,
                OrganizeName = organize.OrganizeName,
                OrganizeAddress = organize.OrganizeAddress,
                OrganizeContact = organize.OrganizeContact,
                OrganizePermission = organize.OrganizePermission,
                CertificationDetails = organize.Certifications.Select(cert => new CertificationDetailsDto
                {
                    CertId = cert.CertId,
                    CertName = cert.CertName,
                    CertCode = cert.CertCode,
                    CertDescription = cert.CertDescription,
                    CertImage = cert.CertImage,
                    TypeName = cert.Type?.TypeName,
                    CertValidity = cert.CertValidity,
                    OrganizeName = cert.Organize?.OrganizeName,
                    Permission = cert.Permission,
                }).ToList()
            };

            return organizeDto;
        }

        public async Task<List<OrganizeDto>> GetOrganizeByNameAsync(string organizeName, CancellationToken cancellationToken)
        {
            var result = await _uow.OrganizeRepository.WhereAsync(
                x => x.OrganizeName.Contains(organizeName),
                cancellationToken,
                include: x => x.Include(o => o.Certifications)
                            .ThenInclude(cert => cert.Type)

            );

            if (!result.Any())
            {
                throw new KeyNotFoundException("Organize not found.");
            }

            var organizeDtos = result.Select(organize => new OrganizeDto
            {
                OrganizeId = organize.OrganizeId,
                OrganizeName = organize.OrganizeName,
                OrganizeAddress = organize.OrganizeAddress,
                OrganizeContact = organize.OrganizeContact,
                OrganizePermission = organize.OrganizePermission,
                CertificationDetails = organize.Certifications.Select(cert => new CertificationDetailsDto
                {
                    CertId = cert.CertId,
                    CertName = cert.CertName,
                    CertCode = cert.CertCode,
                    CertDescription = cert.CertDescription,
                    CertImage = cert.CertImage,
                    TypeName = cert.Type?.TypeName,
                    CertValidity = cert.CertValidity,
                    OrganizeName = cert.Organize?.OrganizeName,
                    Permission = cert.Permission,
                }).ToList()
            }).ToList();

            return organizeDtos;
        }


        public async Task<OrganizeDto> UpdateOrganizeAsync(int oragnizeId, UpdateOrganizeRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateOrganizeValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var organize = await _uow.OrganizeRepository.FirstOrDefaultAsync(x => x.OrganizeId == oragnizeId, cancellationToken);
            if (organize is null) 
            {
                throw new KeyNotFoundException("Organize not found.");
            }
            organize.OrganizeName = request.OrganizeName;
            organize.OrganizeContact = request.OrganizeContact;
            organize.OrganizeAddress = request.OrganizeAddress?.Trim();
            organize.OrganizePermission = "Pending";

            _uow.OrganizeRepository.Update(organize);
            await _uow.Commit(cancellationToken);

            // Create the notification
            var notification = new Notification
            {
                NotificationName = "Organization Updated",
                NotificationDescription = $"The organization '{organize.OrganizeName}' has been updated and is pending approval.",
                NotificationImage = null,
                CreationDate = DateTime.UtcNow,
                Role = "manager",
                IsRead = false,
                NotificationType = "organizations",
                NotificationTypeId = organize.OrganizeId,

            };

            // Add the notification to the repository
            await _uow.NotificationRepository.AddAsync(notification);
            await _uow.Commit(cancellationToken);

            var notifications = await _notificationService.GetNotificationByRoleAsync("manager", new CancellationToken());
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);

            return _mapper.Map<OrganizeDto>(organize);
        }

        public async Task<OrganizeDto> UpdateOrganizePermissionAsync(int organizeId, EnumPermission organizePermission, CancellationToken cancellationToken)
        {
            var organize = await _uow.OrganizeRepository.FirstOrDefaultAsync(x => x.OrganizeId == organizeId, cancellationToken);

            if (organize is null)
            {
                throw new KeyNotFoundException("Organize not found.");
            }

            organize.OrganizePermission = organizePermission.ToString();

            _uow.OrganizeRepository.Update(organize);
            await _uow.Commit(cancellationToken);
            var notification = new Notification
            {
                NotificationName = "Organize Permission Update",
                NotificationDescription = $"The organization '{organize.OrganizeName}' has been {organizePermission}.",
                CreationDate = DateTime.UtcNow,
                Role = "staff",
                IsRead = false,
                NotificationType = "organizations",
                NotificationTypeId = organize.OrganizeId,

            };

            await _uow.NotificationRepository.AddAsync(notification);
            await _uow.Commit(cancellationToken);


            var notifications = await _notificationService.GetNotificationByRoleAsync("staff", new CancellationToken());
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);
            var result = _mapper.Map<OrganizeDto>(organize);

            return result;
        }
    }
}
