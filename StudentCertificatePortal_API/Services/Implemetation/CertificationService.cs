using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
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

        public CertificationService(IUnitOfWork uow, IMapper mapper,
            IValidator<CreateCertificationRequest> addCertificationValidator,
            IValidator<UpdateCertificationRequest> updateCertificationValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _addCertificationValidator = addCertificationValidator;
            _updateCertificationValidator = updateCertificationValidator;
        }

        public async Task<CertificationDto> CreateCertificationAsync(CreateCertificationRequest request, CancellationToken cancellationToken)
        {
            // Validate the request
            var validation = await _addCertificationValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            // Create a new certification entity from the request
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
                OrganizeId = request.OrganizeId
            };

            // Check if CertPrerequisites exists, is not null or empty, and does not contain invalid values (e.g., 0)
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


            try
            {
                await _uow.Commit(cancellationToken);

                var organize = await _uow.OrganizeRepository.FirstOrDefaultAsync(x => x.OrganizeId == certificationEntity.OrganizeId);


                var type = await _uow.CertTypeRepository.FirstOrDefaultAsync(x => x.TypeId == certificationEntity.TypeId);

                // Create the DTO and populate the prerequisite names
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

                // Create the DTO aand populate the major 

                certificationDto.MajorIds = certificationEntity.Majors.Select(major => major.MajorId).ToList();
                certificationDto.MajorCodes = certificationEntity.Majors.Select(major => major.MajorCode).ToList();
                certificationDto.MajorNames = certificationEntity.Majors.Select(major => major.MajorName).ToList();
                certificationDto.MajorDescriptions = certificationEntity.Majors.Select(major => major.MajorDescription).ToList();

                // Create the DTO aand populate the JobPosition 

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
            // Retrieve the certification with its prerequisites
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

            // Remove all prerequisites where this certification is listed as a prerequisite
            certification.CertIdPrerequisites?.Clear();
            certification.Courses?.Clear();
            certification.ExamSessions?.Clear();
            certification.JobPositions?.Clear();
            certification.SimulationExams?.Clear();
            certification.Majors?.Clear();

            // Find dependent certifications that have this certification as a prerequisite
            var dependentCertifications = await _uow.CertificationRepository.WhereAsync(
                x => x.CertIdPrerequisites.Any(p => p.CertId == certificationId),
                cancellationToken
            );

            foreach (var dependentCert in dependentCertifications)
            {
                // Reload the dependent certification with its prerequisites if needed
                var fullDependentCert = await _uow.CertificationRepository.FirstOrDefaultAsync(
                    x => x.CertId == dependentCert.CertId,
                    cancellationToken,
                    include: q => q.Include(c => c.CertIdPrerequisites)
                                    .Include(c => c.Majors)
                                    .Include(c => c.JobPositions)
                );

                // Remove the specific prerequisite reference
                var prerequisitesToRemove = fullDependentCert.CertIdPrerequisites
                    .Where(p => p.CertId == certificationId)
                    .ToList();

                foreach (var prerequisite in prerequisitesToRemove)
                {
                    fullDependentCert.CertIdPrerequisites.Remove(prerequisite);
                }

                // Save the changes for each dependent certification
                _uow.CertificationRepository.Update(fullDependentCert);
            }

            // Commit changes to remove all prerequisites and dependent relationships first
            await _uow.Commit(cancellationToken);

            // Now delete the main certification
            _uow.CertificationRepository.Delete(certification);
            await _uow.Commit(cancellationToken);

            // Map the deleted certification to DTO
            var certificationDto = _mapper.Map<CertificationDto>(certification);
            return certificationDto;
        }





        public async Task<List<CertificationDto>> GetAll()
        {
            var certifications = await _uow.CertificationRepository.GetAllAsync(query =>
                query.Include(c => c.CertIdPrerequisites)
                .Include(c => c.Majors)
                .Include(c => c.JobPositions)); // Eager loading prerequisites

            var certificationDtos = new List<CertificationDto>();

            foreach (var certification in certifications)
            {
                var certificationDto = _mapper.Map<CertificationDto>(certification);

                var organize = await _uow.OrganizeRepository.FirstOrDefaultAsync(x => x.OrganizeId == certification.OrganizeId);
                var type = await _uow.CertTypeRepository.FirstOrDefaultAsync(x => x.TypeId == certification.TypeId);

                certificationDto.OrganizeId = organize?.OrganizeId;
                certificationDto.OrganizeName = organize?.OrganizeName;
                certificationDto.TypeId = type?.TypeId;
                certificationDto.TypeName = type?.TypeName; // Null check added

                // Manually map prerequisite details
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


                // Create the DTO aand populate the major 

                certificationDto.MajorIds = certification.Majors.Select(major => major.MajorId).ToList();
                certificationDto.MajorCodes = certification.Majors.Select(major => major.MajorCode).ToList();
                certificationDto.MajorNames = certification.Majors.Select(major => major.MajorName).ToList();
                certificationDto.MajorDescriptions = certification.Majors.Select(major => major.MajorDescription).ToList();

                // Create the DTO aand populate the JobPosition 

                certificationDto.JobPositionIds = certification.JobPositions.Select(job => job.JobPositionId).ToList();
                certificationDto.JobPositionCodes = certification.JobPositions.Select(job => job.JobPositionCode).ToList();
                certificationDto.JobPositionNames = certification.JobPositions.Select(job => job.JobPositionName).ToList();
                certificationDto.JobPositionDescriptions = certification.JobPositions.Select(major => major.JobPositionDescription).ToList();
                // Add the result to the list
                certificationDtos.Add(certificationDto);
            }

            return certificationDtos;
        }



        public async Task<CertificationDto> GetCertificationById(int certificationId, CancellationToken cancellationToken)
        {
            // Eager load the prerequisite certifications using Include
            var certification = await _uow.CertificationRepository
                .FirstOrDefaultAsync(
                    x => x.CertId == certificationId,
                    cancellationToken: cancellationToken,
                    include: query => query.Include(c => c.CertIdPrerequisites)
                                      .Include(c => c.Majors)
                                      .Include(c => c.JobPositions)// Include prerequisites
                );

            if (certification is null)
            {
                throw new KeyNotFoundException("Certification not found.");
            }

            var organize = await _uow.OrganizeRepository.FirstOrDefaultAsync(x => x.OrganizeId == certification.OrganizeId);
            var type = await _uow.CertTypeRepository.FirstOrDefaultAsync(x => x.TypeId == certification.TypeId);

            
            // Map the certification to DTO
            var certificationDto = _mapper.Map<CertificationDto>(certification);


            certificationDto.OrganizeId = organize?.OrganizeId;
            certificationDto.OrganizeName = organize?.OrganizeName;
            certificationDto.TypeId = type?.TypeId;
            certificationDto.TypeName = type?.TypeName;
            // Populate the prerequisite details
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

            // Create the DTO aand populate the major 

            certificationDto.MajorIds = certification.Majors.Select(major => major.MajorId).ToList();
            certificationDto.MajorCodes = certification.Majors.Select(major => major.MajorCode).ToList();
            certificationDto.MajorNames = certification.Majors.Select(major => major.MajorName).ToList();
            certificationDto.MajorDescriptions = certification.Majors.Select(major => major.MajorDescription).ToList();

            // Create the DTO aand populate the JobPosition 

            certificationDto.JobPositionIds = certification.JobPositions.Select(job => job.JobPositionId).ToList();
            certificationDto.JobPositionCodes = certification.JobPositions.Select(job => job.JobPositionCode).ToList();
            certificationDto.JobPositionNames = certification.JobPositions.Select(job => job.JobPositionName).ToList();
            certificationDto.JobPositionDescriptions = certification.JobPositions.Select(major => major.JobPositionDescription).ToList();

            return certificationDto;
        }

        public async Task<List<CertificationDto>> GetCertificationByNameAsync(string certName, CancellationToken cancellationToken)
        {
            // Fetch certifications including prerequisites
            var certifications = await _uow.CertificationRepository
                .WhereAsync(
                    x => x.CertName.Contains(certName),
                    cancellationToken,
                    include: query => query.Include(c => c.CertIdPrerequisites)
                );

            // If no certifications found, throw an exception
            if (certifications == null || !certifications.Any())
            {
                throw new KeyNotFoundException("No certifications found with the given name.");
            }

            // Map the certifications to DTOs
            var certificationDtos = _mapper.Map<List<CertificationDto>>(certifications);

            foreach (var certificationDto in certificationDtos)
            {
                // Find the original certification entity
                var certification = certifications.First(c => c.CertId == certificationDto.CertId);

                // Fetch related organize and cert type
                var organize = await _uow.OrganizeRepository.FirstOrDefaultAsync(x => x.OrganizeId == certification.OrganizeId);
                var type = await _uow.CertTypeRepository.FirstOrDefaultAsync(x => x.TypeId == certification.TypeId);

                // Fill in prerequisite details
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

                // Include organization and type details in DTO
                certificationDto.OrganizeId = organize?.OrganizeId;
                certificationDto.OrganizeName = organize?.OrganizeName;
                certificationDto.TypeId = type?.TypeId;
                certificationDto.TypeName = type?.TypeName;
            }

            return certificationDtos;
        }



        public async Task<CertificationDto> UpdateCertificationAsync(int certificationId, UpdateCertificationRequest request, CancellationToken cancellationToken)
        {
            // Validate the request
            var validation = await _updateCertificationValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }

            // Retrieve the existing certification entity including prerequisites
            var certification = await _uow.CertificationRepository
                .Include(x => x.CertIdPrerequisites)
                .Include(c => c.Majors)
                .Include(c => c.JobPositions)
                .FirstOrDefaultAsync(x => x.CertId == certificationId, cancellationToken);

            if (certification is null)
            {
                throw new KeyNotFoundException("Certification not found.");
            }

            // Update main certification properties with new values
            certification.CertName = request.CertName;
            certification.CertCode = request.CertCode;
            certification.CertDescription = request.CertDescription;
            certification.CertCost = request.CertCost;
            certification.CertPointSystem = request.CertPointSystem;
            certification.CertImage = request.CertImage;
            certification.CertValidity = request.CertValidity;
            certification.TypeId = request.TypeId;
            certification.OrganizeId = request.OrganizeId;

            // Get existing prerequisite IDs
            var existingPrerequisiteIds = certification.CertIdPrerequisites.Select(p => p.CertId).ToList();
            var newPrerequisiteIds = request.CertIdPrerequisites ?? new List<int>();

            var existingMajorIds = certification.Majors.Select(p => p.MajorId).ToList();
            var newMajorIds = request.MajorIds ?? new List<int>();

            var existingJobPositionIds = certification.JobPositions.Select(p => p.JobPositionId).ToList();
            var newJobPositionIds = request.JobIds ?? new List<int>();
            // Remove prerequisites that are no longer referenced
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
            // Add new prerequisites that are not already in the certification
            foreach (var certPreId in newPrerequisiteIds)
            {
                if (!existingPrerequisiteIds.Contains(certPreId))
                {
                    var prerequisiteCert = await _uow.CertificationRepository
                        .FirstOrDefaultAsync(x => x.CertId == certPreId, cancellationToken);

                    if (prerequisiteCert != null)
                    {
                        // Check if this relationship already exists to avoid duplicates
                        var exists = certification.CertIdPrerequisites.Any(p => p.CertId == certPreId);
                        if (!exists) // Only add if it doesn't already exist
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

            // Update the certification in the repository
            _uow.CertificationRepository.Update(certification);

            try
            {
                await _uow.Commit(cancellationToken);

                // Create the DTO and populate the prerequisite details
                var certificationDto = _mapper.Map<CertificationDto>(certification);

                // Populate the prerequisite details for the DTO
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
                // Major information is shown

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


                // Job Position information is shown
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
    }
}
