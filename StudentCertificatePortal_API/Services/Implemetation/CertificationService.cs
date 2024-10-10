using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
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
                ExpiryDate = request.ExpiryDate,
                TypeId = request.TypeId,
                OrganizeId = request.OrganizeId
            };

            // Check if CertPrerequisites exists, is not null or empty, and does not contain invalid values (e.g., 0)
            if (request.CertIdPrerequisites != null && request.CertIdPrerequisites.Any() && !request.CertIdPrerequisites.Contains(0))
            {
                foreach (var certPreId in request.CertIdPrerequisites)
                {
                    var prerequisiteCert = await _uow.CertificationRepository.FirstOrDefaultAsync(x => x.CertId == certPreId);
                    if (prerequisiteCert != null)
                    {
                        certificationEntity.CertIdPrerequisites.Add(prerequisiteCert);
                        await _uow.CertificationRepository.AddAsync(certificationEntity);
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Prerequisite certification with ID {certPreId} not found.");
                    }
                }
            }

            try
            {
                await _uow.Commit(cancellationToken);

                // Create the DTO and populate the prerequisite names
                var certificationDto = _mapper.Map<CertificationDto>(certificationEntity);
                certificationDto.CertPrerequisite = certificationEntity.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertName)
                    .ToList();

                certificationDto.CertCodePrerequisite = certificationEntity.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertCode)
                    .ToList();

                certificationDto.CertDescriptionPrerequisite = certificationEntity.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertDescription)
                    .ToList();
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
            );

            if (certification is null)
            {
                throw new KeyNotFoundException("Certification not found.");
            }

            // Remove all prerequisites where this certification is listed as a prerequisite
            certification.CertIdPrerequisites?.Clear();

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
                query.Include(c => c.CertIdPrerequisites)); // Eager loading prerequisites

            var certificationDtos = certifications.Select(certification =>
            {
                var certificationDto = _mapper.Map<CertificationDto>(certification);

                // Manually map prerequisite details
                certificationDto.CertPrerequisite = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertName)
                    .ToList();

                certificationDto.CertCodePrerequisite = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertCode)
                    .ToList();

                certificationDto.CertDescriptionPrerequisite = certification.CertIdPrerequisites
                    .Select(prerequisite => prerequisite.CertDescription)
                    .ToList();

                return certificationDto;
            }).ToList();

            return certificationDtos;
        }







        public async Task<CertificationDto> GetCertificationById(int certificationId, CancellationToken cancellationToken)
        {
            // Eager load the prerequisite certifications using Include
            var certification = await _uow.CertificationRepository
                .FirstOrDefaultAsync(
                    x => x.CertId == certificationId,
                    cancellationToken: cancellationToken,
                    include: query => query.Include(c => c.CertIdPrerequisites) // Include prerequisites
                );

            if (certification is null)
            {
                throw new KeyNotFoundException("Certification not found.");
            }

            // Map the certification to DTO
            var certificationDto = _mapper.Map<CertificationDto>(certification);

            // Populate the prerequisite details
            certificationDto.CertPrerequisite = certification.CertIdPrerequisites
                .Select(prerequisite => prerequisite.CertName)
                .ToList();

            certificationDto.CertCodePrerequisite = certification.CertIdPrerequisites
                .Select(prerequisite => prerequisite.CertCode)
                .ToList();

            certificationDto.CertDescriptionPrerequisite = certification.CertIdPrerequisites
                .Select(prerequisite => prerequisite.CertDescription)
                .ToList();

            return certificationDto;
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
                .Include(x => x.CertIdPrerequisites) // Ensure prerequisites are included
                .FirstOrDefaultAsync(x => x.CertId == certificationId, cancellationToken)
                .ConfigureAwait(false);

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
            certification.ExpiryDate = request.ExpiryDate;
            certification.TypeId = request.TypeId;
            certification.OrganizeId = request.OrganizeId;

            // Get existing prerequisite IDs
            var existingPrerequisiteIds = certification.CertIdPrerequisites.Select(p => p.CertId).ToList();
            var newPrerequisiteIds = request.CertIdPrerequisites ?? new List<int>();

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
