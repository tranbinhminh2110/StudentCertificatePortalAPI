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
using System.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class CertificationService : ICertificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateCertificationRequest> _addCertificationValidator;
        private readonly IValidator<UpdateCertificationRequest> _updateCertificationValidator;


        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;

        public CertificationService(IUnitOfWork uow, IMapper mapper,
            IValidator<CreateCertificationRequest> addCertificationValidator,
            IValidator<UpdateCertificationRequest> updateCertificationValidator, 
            IHubContext<NotificationHub> hubContext, INotificationService notificationService)
        {
            _uow = uow;
            _mapper = mapper;
            _addCertificationValidator = addCertificationValidator;
            _updateCertificationValidator = updateCertificationValidator;
            _hubContext = hubContext;
            _notificationService = notificationService;
        }

        public async Task<CertificationDto> CreateCertificationAsync(CreateCertificationRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addCertificationValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            var certificationEntity = new Certification()
            {
                CertName = request.CertName,
                CertCode = request.CertCode,
                CertCost = request.CertCost,
                CertDescription = request.CertDescription,
                CertPointSystem = request.CertPointSystem,
                CertImage = request.CertImage,
                CertValidity = request.CertValidity,
                TypeId = request.TypeId,
                OrganizeId = request.OrganizeId,
                Permission = Enums.EnumPermission.Pending.ToString(),
            };

            
            if (request.CertIdPrerequisites.Any())
            {
                foreach (var certPreId in request.CertIdPrerequisites)
                {
                    var prerequisiteCert = await _uow.CertificationRepository.FirstOrDefaultAsync(x => x.CertId == certPreId);
                    if (prerequisiteCert != null)
                    {
                        certificationEntity.CertIdPrerequisites.Add(prerequisiteCert);
                        
                    }
                    
                }
            }

            if(request.MajorIds.Any()) { 
                foreach(var majorId in request.MajorIds)
                {
                    var major = await _uow.MajorRepository.FirstOrDefaultAsync(x => x.MajorId == majorId);
                    if(major != null)
                    {
                        certificationEntity.Majors.Add(major);
                    }
            
                }
            }

            if(request.JobIds.Any())
            {
                foreach(var jobId in request.JobIds)
                {
                    var job = await _uow.JobPositionRepository.FirstOrDefaultAsync(x => x.JobPositionId == jobId);
                    if(job != null)
                    {
                        certificationEntity.JobPositions.Add(job);
                    }
                }
            }

            await _uow.CertificationRepository.AddAsync(certificationEntity);
            await _uow.Commit(cancellationToken);
            var notification = new Notification()
            {
                NotificationName = "New Certification Created",
                NotificationDescription = $"A new certification '{request.CertName}' has been created and is pending approval.",
                NotificationImage = request.CertImage,
                CreationDate = DateTime.UtcNow,
                Role = "manager",
                IsRead = false,
                NotificationType = "certificate",
                NotificationTypeId = certificationEntity.CertId,
            };
            await _uow.NotificationRepository.AddAsync(notification);
            await _uow.Commit(cancellationToken);
            var notifications = await _notificationService.GetNotificationByRoleAsync("manager", new CancellationToken());
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);

            try
            {
                

                var organize = await _uow.OrganizeRepository.FirstOrDefaultAsync(x => x.OrganizeId == certificationEntity.OrganizeId);


                var type = await _uow.CertTypeRepository.FirstOrDefaultAsync(x => x.TypeId == certificationEntity.TypeId);

                var certificationDto = _mapper.Map<CertificationDto>(certificationEntity);
                certificationDto.CertPrerequisiteId = certificationEntity.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertId)
                    .ToList();

                certificationDto.CertPrerequisite = certificationEntity.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertName)
                    .ToList();

                certificationDto.CertCodePrerequisite = certificationEntity.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertCode)
                    .ToList();

                certificationDto.CertDescriptionPrerequisite = certificationEntity.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertDescription)
                    .ToList();

                 

                certificationDto.MajorIds = certificationEntity.Majors.Select(major => major.MajorId).ToList();
                certificationDto.MajorCodes = certificationEntity.Majors.Select(major => major.MajorCode).ToList();
                certificationDto.MajorNames = certificationEntity.Majors.Select(major => major.MajorName).ToList();
                certificationDto.MajorDescriptions = certificationEntity.Majors.Select(major => major.MajorDescription).ToList();

                

                certificationDto.JobPositionIds = certificationEntity.JobPositions.Select(job => job.JobPositionId).ToList();
                certificationDto.JobPositionCodes = certificationEntity.JobPositions.Select(job => job.JobPositionCode).ToList();
                certificationDto.JobPositionNames = certificationEntity.JobPositions.Select(job => job.JobPositionName).ToList();
                certificationDto.JobPositionDescriptions = certificationEntity.JobPositions.Select(major => major.JobPositionDescription).ToList();
                certificationDto.OrganizeId = organize?.OrganizeId;
                certificationDto.OrganizeName = organize?.OrganizeName;
                certificationDto.TypeId = type?.TypeId;
                certificationDto.TypeName = type?.TypeName;
                return certificationDto;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? "No inner exception";
                throw new Exception($"An error occurred while saving the entity changes: {innerExceptionMessage}", ex);
            }
        }





        public async Task<CertificationDto> DeleteCertificationAsync(int certificationId, CancellationToken cancellationToken)
        {
            
            var certification = await _uow.CertificationRepository.FirstOrDefaultAsync(
                                        x => x.CertId == certificationId,
                                        cancellationToken,
                                        include: q => q.Include(c => c.CertIdPrerequisites)
                                                       .Include(c => c.Courses)
                                                       .Include(c => c.ExamSessions)
                        .Include(c => c.JobPositions)
                        .Include(c => c.SimulationExams)
                        .Include(c => c.Majors)
    );


            if (certification is null)
            {
                throw new KeyNotFoundException("Certification not found.");
            }

            
            certification.CertIdPrerequisites?.Clear();
            certification.Courses?.Clear();
            certification.ExamSessions?.Clear();
            certification.JobPositions?.Clear();
            certification.SimulationExams?.Clear();
            certification.Majors?.Clear();

            
            var dependentCertifications = await _uow.CertificationRepository.WhereAsync(
                x => x.CertIdPrerequisites.Any(p => p.CertId == certificationId),
                cancellationToken
            );

            foreach (var dependentCert in dependentCertifications)
            {
                
                var fullDependentCert = await _uow.CertificationRepository.FirstOrDefaultAsync(
                    x => x.CertId == dependentCert.CertId,
                    cancellationToken,
                    include: q => q.Include(c => c.CertIdPrerequisites)
                                    .Include(c => c.Majors)
                                    .Include(c => c.JobPositions)
                );

               
                var prerequisitesToRemove = fullDependentCert.CertIdPrerequisites
                    .Where(p => p.CertId == certificationId)
                    .ToList();

                foreach (var prerequisite in prerequisitesToRemove)
                {
                    fullDependentCert.CertIdPrerequisites.Remove(prerequisite);
                }

                
                _uow.CertificationRepository.Update(fullDependentCert);
            }

            
            await _uow.Commit(cancellationToken);

            
            _uow.CertificationRepository.Delete(certification);
            await _uow.Commit(cancellationToken);

            
            var certificationDto = _mapper.Map<CertificationDto>(certification);
            return certificationDto;
        }





        public async Task<List<CertificationDto>> GetAll()
        {
            var certifications = await _uow.CertificationRepository.GetAllAsync(query =>
                query.Include(c => c.CertIdPrerequisites)
                .Include(c => c.Majors)
                .Include(c => c.JobPositions).Include(x => x.Certs)); 

            var certificationDtos = new List<CertificationDto>();

            foreach (var certification in certifications)
            {
                var certificationDto = _mapper.Map<CertificationDto>(certification);

                var organize = await _uow.OrganizeRepository.FirstOrDefaultAsync(x => x.OrganizeId == certification.OrganizeId);
                var type = await _uow.CertTypeRepository.FirstOrDefaultAsync(x => x.TypeId == certification.TypeId);


                var certificationsSub = await _uow.CertificationRepository.WhereAsync(cert => cert.CertId == certification.CertId);
                var subsequent = certifications.Select(cert => cert.Certs);



                certificationDto.OrganizeId = organize?.OrganizeId;
                certificationDto.OrganizeName = organize?.OrganizeName;
                certificationDto.TypeId = type?.TypeId;
                certificationDto.TypeName = type?.TypeName;
                var subsequentCertIds = certification.Certs.Select(cert => cert.CertId).ToList();
                var subsequentCertNames = certification.Certs.Select(cert => cert.CertName).ToList();
                certificationDto.CertSubsequentIds = subsequentCertIds;
                certificationDto.CertSubsequentNames = subsequentCertNames;
                certificationDto.CertPrerequisiteId = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertId)
                    .ToList();
                certificationDto.CertPrerequisite = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertName)
                    .ToList();

                certificationDto.CertCodePrerequisite = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertCode)
                    .ToList();

                certificationDto.CertDescriptionPrerequisite = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertDescription)
                    .ToList();




                certificationDto.MajorIds = certification.Majors.Select(major => major.MajorId).ToList();
                certificationDto.MajorCodes = certification.Majors.Select(major => major.MajorCode).ToList();
                certificationDto.MajorNames = certification.Majors.Select(major => major.MajorName).ToList();
                certificationDto.MajorDescriptions = certification.Majors.Select(major => major.MajorDescription).ToList();
                certificationDto.MajorPermission = certification.Majors.Select(major => major.MajorPermission).ToList();



                certificationDto.JobPositionIds = certification.JobPositions.Select(job => job.JobPositionId).ToList();
                certificationDto.JobPositionCodes = certification.JobPositions.Select(job => job.JobPositionCode).ToList();
                certificationDto.JobPositionNames = certification.JobPositions.Select(job => job.JobPositionName).ToList();
                certificationDto.JobPositionDescriptions = certification.JobPositions.Select(major => major.JobPositionDescription).ToList();
                certificationDto.JobPositionPermission = certification.JobPositions.Select(major => major.JobPositionPermission).ToList();

                certificationDtos.Add(certificationDto);
            }

            return certificationDtos.OrderBy(x => x.CertName).ToList();
        }



        public async Task<CertificationDto> GetCertificationById(int certificationId, CancellationToken cancellationToken)
        {

            var certification = await _uow.CertificationRepository
                .FirstOrDefaultAsync(
                    x => x.CertId == certificationId,
                    cancellationToken: cancellationToken,
                    include: query => query.Include(c => c.CertIdPrerequisites)
                                      .Include(c => c.Majors)
                                      .Include(c => c.JobPositions).Include(c => c.Certs)
                );

            if (certification is null)
            {
                throw new KeyNotFoundException("Certification not found.");
            }

            var organize = await _uow.OrganizeRepository.FirstOrDefaultAsync(x => x.OrganizeId == certification.OrganizeId);
            var type = await _uow.CertTypeRepository.FirstOrDefaultAsync(x => x.TypeId == certification.TypeId);

            
            var certificationDto = _mapper.Map<CertificationDto>(certification);


            certificationDto.OrganizeId = organize?.OrganizeId;
            certificationDto.OrganizeName = organize?.OrganizeName;
            certificationDto.TypeId = type?.TypeId;
            certificationDto.TypeName = type?.TypeName;


            var subsequentCertIds = certification.Certs.Select(cert => cert.CertId).ToList();
            var subsequentCertNames = certification.Certs.Select(cert => cert.CertName).ToList();
            certificationDto.CertSubsequentIds = subsequentCertIds;
            certificationDto.CertSubsequentNames = subsequentCertNames;

            certificationDto.CertPrerequisiteId = certification.CertIdPrerequisites
                .Select(prerequisite => prerequisite.CertId)
                .ToList();
            certificationDto.CertPrerequisite = certification.CertIdPrerequisites
                .Select(prerequisite => prerequisite.CertName)
                .ToList();

            certificationDto.CertCodePrerequisite = certification.CertIdPrerequisites
                .Select(prerequisite => prerequisite.CertCode)
                .ToList();

            certificationDto.CertDescriptionPrerequisite = certification.CertIdPrerequisites
                .Select(prerequisite => prerequisite.CertDescription)
                .ToList();

            

            certificationDto.MajorIds = certification.Majors.Select(major => major.MajorId).ToList();
            certificationDto.MajorCodes = certification.Majors.Select(major => major.MajorCode).ToList();
            certificationDto.MajorNames = certification.Majors.Select(major => major.MajorName).ToList();
            certificationDto.MajorDescriptions = certification.Majors.Select(major => major.MajorDescription).ToList();
            certificationDto.MajorPermission = certification.Majors.Select(major => major.MajorPermission).ToList();



            certificationDto.JobPositionIds = certification.JobPositions.Select(job => job.JobPositionId).ToList();
            certificationDto.JobPositionCodes = certification.JobPositions.Select(job => job.JobPositionCode).ToList();
            certificationDto.JobPositionNames = certification.JobPositions.Select(job => job.JobPositionName).ToList();
            certificationDto.JobPositionDescriptions = certification.JobPositions.Select(major => major.JobPositionDescription).ToList();
            certificationDto.JobPositionPermission = certification.JobPositions.Select(major => major.JobPositionPermission).ToList();

            return certificationDto;
        }

        public async Task<List<CertificationDto>> GetCertificationByNameAsync(string certName, CancellationToken cancellationToken)
        {
            
            var certifications = await _uow.CertificationRepository
                .WhereAsync(
                    x => x.CertName.Contains(certName),
                    cancellationToken,
                    include: query => query.Include(c => c.CertIdPrerequisites).Include(x => x.Certs)
                );

            
            if (certifications == null || !certifications.Any())
            {
                throw new KeyNotFoundException("No certifications found with the given name.");
            }

            
            var certificationDtos = _mapper.Map<List<CertificationDto>>(certifications);

            foreach (var certificationDto in certificationDtos)
            {
                
                var certification = certifications.First(c => c.CertId == certificationDto.CertId);

                
                var organize = await _uow.OrganizeRepository.FirstOrDefaultAsync(x => x.OrganizeId == certification.OrganizeId);
                var type = await _uow.CertTypeRepository.FirstOrDefaultAsync(x => x.TypeId == certification.TypeId);
                var subsequentCertIds = certification.Certs.Select(cert => cert.CertId).ToList();
                var subsequentCertNames = certification.Certs.Select(cert => cert.CertName).ToList();
                certificationDto.CertSubsequentIds = subsequentCertIds;
                certificationDto.CertSubsequentNames = subsequentCertNames;

                certificationDto.CertPrerequisiteId = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertId)
                    .ToList();
                certificationDto.CertPrerequisite = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertName)
                    .ToList();

                certificationDto.CertCodePrerequisite = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertCode)
                    .ToList();

                certificationDto.CertDescriptionPrerequisite = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertDescription)
                    .ToList();

                
                certificationDto.OrganizeId = organize?.OrganizeId;
                certificationDto.OrganizeName = organize?.OrganizeName;
                certificationDto.TypeId = type?.TypeId;
                certificationDto.TypeName = type?.TypeName;
            }

            return certificationDtos.OrderBy(x => x.CertName).ToList();
        }



        public async Task<CertificationDto> UpdateCertificationAsync(int certificationId, UpdateCertificationRequest request, CancellationToken cancellationToken)
        {
            
            var validation = await _updateCertificationValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            
            var certification = await _uow.CertificationRepository
                .Include(x => x.CertIdPrerequisites)
                .Include(c => c.Majors)
                .Include(c => c.JobPositions)
                .FirstOrDefaultAsync(x => x.CertId == certificationId, cancellationToken);

            if (certification is null)
            {
                throw new KeyNotFoundException("Certification not found.");
            }

            
            certification.CertName = request.CertName;
            certification.CertCode = request.CertCode;
            certification.CertDescription = request.CertDescription;
            certification.CertCost = request.CertCost;
            certification.CertPointSystem = request.CertPointSystem;
            certification.CertImage = request.CertImage;
            certification.CertValidity = request.CertValidity;
            certification.TypeId = request.TypeId;
            certification.OrganizeId = request.OrganizeId;
            certification.Permission = Enums.EnumPermission.Pending.ToString();

            
            var existingPrerequisiteIds = certification.CertIdPrerequisites.Select(p => p.CertId).ToList();
            var newPrerequisiteIds = request.CertIdPrerequisites ?? new List<int>();

            var existingMajorIds = certification.Majors.Select(p => p.MajorId).ToList();
            var newMajorIds = request.MajorIds ?? new List<int>();

            var existingJobPositionIds = certification.JobPositions.Select(p => p.JobPositionId).ToList();
            var newJobPositionIds = request.JobIds ?? new List<int>();
            
            foreach (var existingPrerequisiteId in existingPrerequisiteIds)
            {
                if (!newPrerequisiteIds.Contains(existingPrerequisiteId))
                {
                    var prerequisiteToRemove = certification.CertIdPrerequisites.FirstOrDefault(p => p.CertId == existingPrerequisiteId);
                    if (prerequisiteToRemove != null)
                    {
                        certification.CertIdPrerequisites.Remove(prerequisiteToRemove);
                    }
                }
            }

            foreach (var existingMajorId in existingMajorIds)
            {
                if (!newMajorIds.Contains(existingMajorId))
                {
                    var prerequisiteToRemove = certification.Majors.FirstOrDefault(p => p.MajorId == existingMajorId);
                    if (prerequisiteToRemove != null)
                    {
                        certification.Majors.Remove(prerequisiteToRemove);
                    }
                }
            }

            foreach (var existingJobPositionId in existingJobPositionIds)
            {
                if (!newJobPositionIds.Contains(existingJobPositionId))
                {
                    var prerequisiteToRemove = certification.JobPositions.FirstOrDefault(p => p.JobPositionId == existingJobPositionId);
                    if (prerequisiteToRemove != null)
                    {
                        certification.JobPositions.Remove(prerequisiteToRemove);
                    }
                }
            }
            
            foreach (var certPreId in newPrerequisiteIds)
            {
                if (!existingPrerequisiteIds.Contains(certPreId))
                {
                    var prerequisiteCert = await _uow.CertificationRepository
                        .FirstOrDefaultAsync(x => x.CertId == certPreId, cancellationToken);

                    if (prerequisiteCert != null)
                    {
                        
                        var exists = certification.CertIdPrerequisites.Any(p => p.CertId == certPreId);
                        if (!exists) 
                        {
                            certification.CertIdPrerequisites.Add(prerequisiteCert);
                        }
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Prerequisite certification with ID {certPreId} not found.");
                    }
                }
            }

            var majorsToAdd = await _uow.MajorRepository
                .WhereAsync(x => newMajorIds.Contains(x.MajorId) && !existingMajorIds.Contains(x.MajorId));
            foreach (var major in majorsToAdd)
            {
                certification.Majors.Add(major);
            }

            var jobPositionsToAdd = await _uow.JobPositionRepository
                .WhereAsync(x => newJobPositionIds.Contains(x.JobPositionId) && !existingJobPositionIds.Contains(x.JobPositionId));

            foreach (var job in jobPositionsToAdd)
            {
                certification.JobPositions.Add(job);
            }

            
            _uow.CertificationRepository.Update(certification);

            try
            {
                await _uow.Commit(cancellationToken);
                var notification = new Notification()
                {
                    NotificationName = "Certification Updated",
                    NotificationDescription = $"The certification '{certification.CertName}' has been updated and is pending approval.",
                    NotificationImage = request.CertImage,
                    CreationDate = DateTime.UtcNow,
                    Role = "manager",
                    IsRead = false,
                    NotificationType = "certificate",
                    NotificationTypeId = certification.CertId,
                };
                await _uow.NotificationRepository.AddAsync(notification);
                await _uow.Commit(cancellationToken);

                var notifications = await _notificationService.GetNotificationByRoleAsync("manager", new CancellationToken());
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);

                var certificationDto = _mapper.Map<CertificationDto>(certification);

                
                certificationDto.CertPrerequisite = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertName)
                    .ToList();

                certificationDto.CertCodePrerequisite = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertCode)
                    .ToList();

                certificationDto.CertDescriptionPrerequisite = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertDescription)
                    .ToList();

                certificationDto.CertDescriptionPrerequisite = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertDescription)
                    .ToList();
                

                certificationDto.MajorIds = certification.Majors
                    .Select(major => major.MajorId)
                    .ToList();
                certificationDto.MajorCodes = certification.Majors
                    .Select(major => major.MajorCode)
                    .ToList();
                certificationDto.MajorDescriptions = certification.Majors
                    .Select(major => major.MajorDescription)
                    .ToList();

                certificationDto.MajorNames = certification.Majors
                    .Select(major => major.MajorName)
                    .ToList();


                
                certificationDto.JobPositionIds = certification.JobPositions
                    .Select(job => job.JobPositionId)
                    .ToList();

                certificationDto.JobPositionCodes = certification.JobPositions
                    .Select(job => job.JobPositionCode)
                    .ToList();
                certificationDto.JobPositionDescriptions = certification.JobPositions
                    .Select(job => job.JobPositionDescription)
                    .ToList();
                certificationDto.JobPositionNames = certification.JobPositions
                    .Select(job => job.JobPositionName)
                    .ToList();



                return certificationDto;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("The certification you're trying to update has been modified by another user. Please try again.", ex);
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
        public async Task<decimal> GetTotalCertCostAsync(List<int> certificationIds, CancellationToken cancellationToken)
        {
            if (certificationIds == null || certificationIds.Count == 0)
            {
                throw new ArgumentException("Certification IDs cannot be empty.");
            }

            var certifications = await _uow.CertificationRepository
                .GetAllAsync(query => query.Where(c => certificationIds.Contains(c.CertId)));

            var notFoundCertIds = certificationIds.Except(certifications.Select(c => c.CertId)).ToList();

            if (notFoundCertIds.Any())
            {
                throw new KeyNotFoundException($"Certifications with IDs {string.Join(", ", notFoundCertIds)} were not found.");
            }

            decimal totalCost = certifications.Sum(c => c.CertCost ?? 0);

            return totalCost;
        }

    }
}
