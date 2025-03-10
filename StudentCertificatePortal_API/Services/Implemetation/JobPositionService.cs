﻿using AutoMapper;
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
    public class JobPositionService : IJobPositionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateJobPositionRequest> _addJobPositonValidator;
        private readonly IValidator<UpdateJobPositionRequest> _updateJobPositonValidator;

        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;

        public JobPositionService(IUnitOfWork uow, IMapper mapper, IValidator<CreateJobPositionRequest> addJobPositionValidator
            , IValidator<UpdateJobPositionRequest> updateJobPositionValidator
            , IHubContext<NotificationHub> hubContext, INotificationService notificationService)
        {
            _uow = uow;
            _mapper = mapper;
            _addJobPositonValidator = addJobPositionValidator;
            _updateJobPositonValidator = updateJobPositionValidator;
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        public async Task<JobPositionDto> CreateJobPositionAsync(CreateJobPositionRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addJobPositonValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var jobEntity = new JobPosition()
            {
                JobPositionId = new Random().Next(1, 10000),
                JobPositionCode = request.JobPositionCode,
                JobPositionName = request.JobPositionName,
                JobPositionDescription = request.JobPositionDescription,
                JobPositionPermission = "Pending",
            };
            if (request.MajorId != null && request.MajorId.Any())
            {
                foreach (var majorId in request.MajorId)
                {
                    var major = await _uow.MajorRepository.FirstOrDefaultAsync(
                        x => x.MajorId == majorId, cancellationToken);

                    if (major != null)
                    {
                        jobEntity.Majors.Add(major); // Add majors to the job position
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Major with ID {majorId} not found.");
                    }
                }
            }
            // Handle the relationship with certifications
            if (request.CertId != null && request.CertId.Any())
            {
                foreach (var certId in request.CertId)
                {
                    var certification = await _uow.CertificationRepository.FirstOrDefaultAsync(
                        x => x.CertId == certId, cancellationToken);

                    if (certification != null)
                    {
                        jobEntity.Certs.Add(certification); // Add majors to the job position
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Certification with ID {certId} not found.");
                    }
                }
            }
            // Save the job position to the repository
            await _uow.JobPositionRepository.AddAsync(jobEntity);
            await _uow.Commit(cancellationToken);
            var notification = new Notification()
            {
                NotificationName = "New Job Position Created",
                NotificationDescription = $"A new job position '{request.JobPositionName}' has been created and is pending approval.",
                NotificationImage = null,
                CreationDate = DateTime.UtcNow,
                Role = "manager",
                IsRead = false,
                NotificationType = "jobPosition",
                NotificationTypeId = jobEntity.JobPositionId,

            };
            await _uow.NotificationRepository.AddAsync(notification);

            await _uow.Commit(cancellationToken);

            var notifications = await _notificationService.GetNotificationByRoleAsync("manager", new CancellationToken());
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);
            // Map the result to a DTO and return it
            var jobPositionDto = _mapper.Map<JobPositionDto>(jobEntity);
            jobPositionDto.MajorId = jobEntity.Majors.Select(m => m.MajorId).ToList();
            jobPositionDto.CertId = jobEntity.Certs.Select(c => c.CertId).ToList();

            return jobPositionDto;
        }

        public async Task<JobPositionDto> DeleteJobPositionAsync(int jobPositionId, CancellationToken cancellationToken)
        {
            var job = await _uow.JobPositionRepository.FirstOrDefaultAsync(x => x.JobPositionId == jobPositionId, cancellationToken,
                include: x => x.Include(c => c.Majors)
                               .Include(c => c.Certs));
            if (job is null)
            {
                throw new KeyNotFoundException("JobPosition not found.");
            }
            job.Majors?.Clear();
            job.Certs?.Clear();

            _uow.JobPositionRepository.Delete(job);
            await _uow.Commit(cancellationToken);

            var jobDto = _mapper.Map<JobPositionDto>(job);
            return jobDto;
        }

        public async Task<List<JobPositionDto>> GetAll()
        {
            var result = await _uow.JobPositionRepository.GetAllAsync(query =>
            query.Include(c => c.Majors)
                .Include(c => c.Certs)
                .ThenInclude(cert => cert.Type)
                .Include(c => c.Certs)
                .ThenInclude(cert => cert.Organize));

            var jobDtos = result.Select(result =>
            {
                var jobDto = _mapper.Map<JobPositionDto>(result);

                jobDto.MajorDetails = result.Majors
                    .Select(major => new MajorDetailsDto
                    {
                        MajorId = major.MajorId,
                        MajorName = major.MajorName,
                        MajorCode = major.MajorCode,
                        MajorDescription = major.MajorDescription,
                        MajorPermission = major.MajorPermission,
                    }).ToList();

                jobDto.CertificationDetails = result.Certs
                    .Select(cert => new CertificationDetailsDto
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
                    }).ToList();

                return jobDto;
            }).ToList();
            return jobDtos;
        }

        public async Task<JobPositionDto> GetJobPositionByIdAsync(int jobPositionId, CancellationToken cancellationToken)
        {
            var result = await _uow.JobPositionRepository.FirstOrDefaultAsync(x => x.JobPositionId == jobPositionId,
                cancellationToken: cancellationToken, include: query => query
                .Include(c => c.Majors)
                .Include(c => c.Certs)
                .ThenInclude(cert => cert.Type)
                .Include(c => c.Certs)
                .ThenInclude(cert => cert.Organize));
            if (result is null)
            {
                throw new KeyNotFoundException("JobPosition not found.");
            }
            var jobDto = _mapper.Map<JobPositionDto>(result);

            jobDto.MajorDetails = result.Majors
                .Select(major => new MajorDetailsDto
                {
                    MajorId = major.MajorId,
                    MajorName = major.MajorName,
                    MajorCode = major.MajorCode,
                    MajorDescription = major.MajorDescription,
                    MajorPermission = major.MajorPermission,
                }).ToList();

            jobDto.CertificationDetails = result.Certs
                .Select(cert => new CertificationDetailsDto
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
                }).ToList();

            return jobDto;
        }

        public async Task<List<JobPositionDto>> GetJobPositionByNameAsync(string jobPositionName, CancellationToken cancellationToken)
        {
            var result = await _uow.JobPositionRepository.WhereAsync(x => x.JobPositionName.Contains(jobPositionName), cancellationToken,
                include: query => query.Include(c => c.Majors)
                                       .Include(c => c.Certs)
                                       .ThenInclude(cert => cert.Type)
                                       .Include(c => c.Certs)
                                       .ThenInclude(cert => cert.Organize));
            if (result is null || !result.Any())
            {
                throw new KeyNotFoundException("Job Position not found.");
            }
            var jobDtos = _mapper.Map<List<JobPositionDto>>(result);
            foreach (var jobDto in jobDtos)
            {
                var job = result.FirstOrDefault(c => c.JobPositionId == jobDto.JobPositionId);
                jobDto.MajorDetails = job.Majors
                    .Select(major => new MajorDetailsDto
                    {
                        MajorId = major.MajorId,
                        MajorName = major.MajorName,
                        MajorCode = major.MajorCode,
                        MajorDescription = major.MajorDescription,
                        MajorPermission = major.MajorPermission,
                    }).ToList();

                jobDto.CertificationDetails = job.Certs
                    .Select(cert => new CertificationDetailsDto
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
                    }).ToList();
            }
            return jobDtos;
        }

        public async Task<JobPositionDto> UpdateJobPositionAsync(int jobPositionId, UpdateJobPositionRequest request, CancellationToken cancellationToken = default)
        {
            var validation = await _updateJobPositonValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var job = await _uow.JobPositionRepository
                .Include(x => x.Majors)
                .Include(c => c.Certs)
                .FirstOrDefaultAsync(x => x.JobPositionId == jobPositionId, cancellationToken)
                .ConfigureAwait(false);
            if (job is null)
            {
                throw new KeyNotFoundException("JobPosition not found.");
            }
            job.JobPositionCode = request.JobPositionCode;
            job.JobPositionName = request.JobPositionName;
            job.JobPositionDescription = request.JobPositionDescription;
            job.JobPositionPermission = "Pending";

            var existingMajorIds = new HashSet<int>(job.Majors.Select(x => x.MajorId));
            var newMajorIds = request.MajorId ?? new List<int>();

            foreach (var existingMajorId in existingMajorIds)
            {
                if (!newMajorIds.Contains(existingMajorId))
                {
                    var majorToRemove = job.Majors.FirstOrDefault(m => m.MajorId == existingMajorId);
                    if (majorToRemove != null)
                    {
                        job.Majors.Remove(majorToRemove);
                    }
                }
            }

            foreach (var newMajorId in newMajorIds)
            {
                if (!existingMajorIds.Contains(newMajorId))
                {
                    var major = await _uow.MajorRepository
                        .FirstOrDefaultAsync(x => x.MajorId == newMajorId, cancellationToken);

                    if (major != null)
                    {
                        if (!job.Majors.Any(m => m.MajorId == newMajorId))
                        {
                            job.Majors.Add(major);
                        }
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Major with ID {newMajorId} not found.");
                    }
                }
            }
            // Update Certs
            var existingCertIds = new HashSet<int>(job.Certs.Select(x => x.CertId));
            var newCertIds = request.CertId ?? new List<int>();

            foreach (var existingCertId in existingCertIds)
            {
                if (!newCertIds.Contains(existingCertId))
                {
                    var certToRemove = job.Certs.FirstOrDefault(c => c.CertId == existingCertId);
                    if (certToRemove != null)
                    {
                        job.Certs.Remove(certToRemove);
                    }
                }
            }

            foreach (var newCertId in newCertIds)
            {
                if (!existingCertIds.Contains(newCertId))
                {
                    var certification = await _uow.CertificationRepository
                        .FirstOrDefaultAsync(x => x.CertId == newCertId, cancellationToken);

                    if (certification != null)
                    {
                        if (!job.Certs.Any(c => c.CertId == newCertId))
                        {
                            job.Certs.Add(certification);
                        }
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Certification with ID {newCertId} not found.");
                    }
                }
            }

            _uow.JobPositionRepository.Update(job);

            try
            {
                await _uow.Commit(cancellationToken);
                var notification = new Notification
                {
                    NotificationName = "Job Position Updated",
                    NotificationDescription = $"The job position '{job.JobPositionName}' has been updated and is pending approval.",
                    NotificationImage = null,
                    CreationDate = DateTime.UtcNow,
                    Role = "manager",
                    IsRead = false,
                    NotificationType = "jobPosition",
                    NotificationTypeId = job.JobPositionId,

                };

                // Add the notification
                await _uow.NotificationRepository.AddAsync(notification);
                await _uow.Commit(cancellationToken);

                var notifications = await _notificationService.GetNotificationByRoleAsync("manager", new CancellationToken());
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);

                var jobPositionDto = _mapper.Map<JobPositionDto>(job);
                jobPositionDto.MajorId = job.Majors.Select(m => m.MajorId).ToList();
                jobPositionDto.CertId = job.Certs.Select(c => c.CertId).ToList();

                return jobPositionDto;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("The JobPosition you're trying to update has been modified by another user. Please try again.", ex);
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

        public async Task<JobPositionDto> UpdateJobPositionPermissionAsync(int jobPositionId, EnumPermission jobPositionPermission, CancellationToken cancellationToken)
        {
            var job = await _uow.JobPositionRepository.FirstOrDefaultAsync(x => x.JobPositionId == jobPositionId, cancellationToken);

            if (job is null)
            {
                throw new KeyNotFoundException("Job Position not found.");
            }

            job.JobPositionPermission = jobPositionPermission.ToString();

            _uow.JobPositionRepository.Update(job);
            await _uow.Commit(cancellationToken);
            var notification = new Notification
            {
                NotificationName = "Job Position Permission Update",
                NotificationDescription = $"The job position '{job.JobPositionName}' has been {jobPositionPermission}.",
                CreationDate = DateTime.UtcNow,
                Role = "staff",
                IsRead = false,
                NotificationType = "jobPosition",
                NotificationTypeId = job.JobPositionId,
            };

            await _uow.NotificationRepository.AddAsync(notification);
            await _uow.Commit(cancellationToken);

            var notifications = await _notificationService.GetNotificationByRoleAsync("staff", new CancellationToken());
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);

            var result = _mapper.Map<JobPositionDto>(job);

            return result;
        }
        public async Task<List<JobPositionTwoIdDto>> GetJobPositionByTwoIdAsync(int jobPositionId, int? organizeId, CancellationToken cancellationToken)
        {
            var results = await _uow.JobPositionRepository.WhereAsync(
                x => x.JobPositionId == jobPositionId,
                cancellationToken: cancellationToken,
                include: query => query.Include(jp => jp.Certs)
                                       .ThenInclude(cert => cert.Type)
                                       .Include(jp => jp.Certs)
                                       .ThenInclude(cert => cert.Organize));

            if (results == null || !results.Any())
            {
                throw new KeyNotFoundException("No job positions found.");
            }

            var jobPositionDtoList = results.Select(result =>
            {
                var jobPositionDto = _mapper.Map<JobPositionTwoIdDto>(result);

                jobPositionDto.CertificationTwoId = result.Certs
                    .Where(cert => cert.JobPositions.Any(j => j.JobPositionId == jobPositionId) &&
                                   (organizeId == null || (cert.Organize != null && cert.Organize.OrganizeId == organizeId)))
                    .Select(cert => new CertificationTwoIdDto
                    {
                        CertId = cert.CertId,
                        CertName = cert.CertName,
                        CertCode = cert.CertCode,
                        CertDescription = cert.CertDescription,
                        CertImage = cert.CertImage,
                        TypeName = cert.Type?.TypeName,
                        CertValidity = cert.CertValidity,
                        OrganizeId = cert.OrganizeId ?? 0,
                        OrganizeName = cert.Organize?.OrganizeName,
                        Permission = cert.Permission,
                    }).ToList();

                jobPositionDto.OrganizeId = organizeId;

                return jobPositionDto;
            }).ToList();

            return jobPositionDtoList;
        }

        public async Task<List<JobPositionDto>> FilterJobPositionByRecommended(int userId, CancellationToken cancellationToken)
        {

            var user = await _uow.UserRepository.FirstOrDefaultAsync(
                x => x.UserId == userId,
                cancellationToken,
                include: u => u.Include(c => c.Certs).ThenInclude(cert => cert.Organize));

            if (user == null || user.Status == false)
            {
                throw new KeyNotFoundException("User not found or deactivated.");
            }

            var userCertIds = user.Certs.Select(c => c.CertId).ToList();
            var userOrganizeIds = user.Certs
                .Where(c => c.Organize != null)
                .Select(c => c.Organize.OrganizeId)
                .Distinct()
                .ToList();

            var recommendedJobs = await _uow.JobPositionRepository.WhereAsync(
                j => j.Certs.Any(cert => userCertIds.Contains(cert.CertId) &&
                                         cert.Organize != null &&
                                         userOrganizeIds.Contains(cert.Organize.OrganizeId)),
                cancellationToken,
                include: j => j.Include(job => job.Certs).ThenInclude(cert => cert.Organize));

            var recommendedJobDtos = recommendedJobs.Select(job => new JobPositionDto
            {
                JobPositionId = job.JobPositionId,
                JobPositionCode = job.JobPositionCode,
                JobPositionName = job.JobPositionName,
                JobPositionDescription = job.JobPositionDescription,
                JobPositionPermission = job.JobPositionPermission,
                CertOrganizes = job.Certs
                    .Where(cert => userCertIds.Contains(cert.CertId) &&
                                   cert.Organize != null &&
                                   userOrganizeIds.Contains(cert.Organize.OrganizeId))
                    .Select(cert => new CertOrganizeDto
                    {
                        CertId = cert.CertId,
                        CertName = cert.CertName,
                        OrganizeId = cert.Organize.OrganizeId,
                        OrganizeName = cert.Organize.OrganizeName
                    })
                    .ToList()
            }).ToList();

            return recommendedJobDtos;
        }


    }
}
