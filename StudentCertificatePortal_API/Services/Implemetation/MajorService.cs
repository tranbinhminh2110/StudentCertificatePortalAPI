using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;
using System.Runtime.ConstrainedExecution;
using System.Security;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class MajorService : IMajorService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateMajorRequest> _addMajorValidator;
        private readonly IValidator<UpdateMajorRequest> _updateMajorValidator;
        public MajorService(IUnitOfWork uow, IMapper mapper, IValidator<CreateMajorRequest> addMajorValidator, IValidator<UpdateMajorRequest> updateMajorValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _addMajorValidator = addMajorValidator;
            _updateMajorValidator = updateMajorValidator;
        }

        public async Task<MajorDto> CreateMajorAsync(CreateMajorRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addMajorValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var majorEntity = new Major()
            {
                MajorCode = request.MajorCode,
                MajorName = request.MajorName,
                MajorDescription = request.MajorDescription,
                MajorImage = request.MajorImage,
                MajorPermission = "Pending",
            };
            if (request.JobPositionId != null && request.JobPositionId.Any())
            {
                foreach (var jobPositionId in request.JobPositionId)
                {
                    var jobPosition = await _uow.JobPositionRepository.FirstOrDefaultAsync(
                        x => x.JobPositionId == jobPositionId, cancellationToken);
                    if (jobPosition != null)
                    {
                        majorEntity.JobPositions.Add(jobPosition);
                    }
                    else
                    {
                        throw new KeyNotFoundException($"JobPosition with ID {jobPositionId} not found.");
                    }

                }
            }
            if (request.CertId != null && request.CertId.Any())
            {
                foreach (var certId in request.CertId)
                {
                    var certification = await _uow.CertificationRepository.FirstOrDefaultAsync(
                        x => x.CertId == certId, cancellationToken);
                    if (certification != null)
                    {
                        majorEntity.Certs.Add(certification);
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Certification with ID {certId} not found.");
                    }
                }
            }
            await _uow.MajorRepository.AddAsync(majorEntity);
            try
            {
                await _uow.Commit(cancellationToken);
                var majorDto = _mapper.Map<MajorDto>(majorEntity);
                majorDto.JobPositionId = majorEntity.JobPositions
                    .Select(majorjob => majorjob.JobPositionId)
                    .ToList();
                majorDto.CertId = majorEntity.Certs
                    .Select(cert => cert.CertId)
                    .ToList();
                return majorDto;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException?.Message ?? "No inner exception";
                throw new Exception($"An error occurred while saving the entity changes: {innerExceptionMessage}", ex);

            }
        }

        public async Task<MajorDto> DeleteMajorAsync(int majorId, CancellationToken cancellationToken)
        {
            var major = await _uow.MajorRepository.FirstOrDefaultAsync(
                x => x.MajorId == majorId,
                cancellationToken, include: q => q.Include(c => c.JobPositions)
                                                  .Include(c => c.Certs));
            if (major is null) 
            {
                throw new KeyNotFoundException("Major not found.");
            }
            major.JobPositions?.Clear();
            major.Certs?.Clear();

            _uow.MajorRepository.Delete(major);
            await _uow.Commit(cancellationToken);

            var majorDto = _mapper.Map<MajorDto>(major);
            return majorDto;
        }

        public async Task<List<MajorDto>> GetAll()
        {
            var result = await _uow.MajorRepository.GetAllAsync(query =>
            query.Include(c => c.JobPositions)
            .Include(c => c.Certs)
            .ThenInclude(cert => cert.Type)
            .Include(c => c.Certs)
            .ThenInclude(cert => cert.Organize));

            var majorDtos = result.Select(major =>
            {
                var majorDto = _mapper.Map<MajorDto>(major);

                majorDto.JobPositionDetails = major.JobPositions
                    .Select(jobPosition => new JobPositionDetailsDto
                    {
                        JobPositionId = jobPosition.JobPositionId,
                        JobPositionName = jobPosition.JobPositionName,
                        JobPositionCode = jobPosition.JobPositionCode,
                        JobPositionDescription = jobPosition.JobPositionDescription,
                        JobPositionPermission = jobPosition.JobPositionPermission,
                    }).ToList();
                majorDto.CertificationDetails = major.Certs
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

                return majorDto;
            }).ToList();

            return majorDtos;
        }

        public async Task<MajorDto> GetMajorByIdAsync(int majorId, CancellationToken cancellationToken)
        {
            var result = await _uow.MajorRepository.FirstOrDefaultAsync(
                x => x.MajorId == majorId, cancellationToken: cancellationToken,include: query => query.Include(c => c.JobPositions)
                            .Include(c => c.Certs)
                            .ThenInclude(cert => cert.Type)
                            .Include(c => c.Certs)
                            .ThenInclude(cert => cert.Organize));
            if (result is null)
            {
                throw new KeyNotFoundException("Major not found.");
            }
            var majorDto = _mapper.Map<MajorDto>(result);

            majorDto.JobPositionDetails = result.JobPositions
                .Select(jobPosition => new JobPositionDetailsDto
                {
                    JobPositionId = jobPosition.JobPositionId,
                    JobPositionName = jobPosition.JobPositionName,
                    JobPositionCode = jobPosition.JobPositionCode,
                    JobPositionDescription = jobPosition.JobPositionDescription,
                    JobPositionPermission = jobPosition.JobPositionPermission,
                    
                }).ToList();
            majorDto.CertificationDetails = result.Certs
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

            return majorDto;
        }

        public async Task<List<MajorDto>> GetMajorByNameAsync(string majorName, CancellationToken cancellationToken)
        {
            var result = await _uow.MajorRepository.WhereAsync(x => x.MajorName.Contains(majorName), cancellationToken,
                    include: query => query.Include(c => c.JobPositions)
                                .Include(c => c.Certs)
                                .ThenInclude(cert => cert.Type)
                                .Include(c => c.Certs)
                                .ThenInclude(cert => cert.Organize));
            if (result is null || !result.Any())
            {
                throw new KeyNotFoundException("Major not found.");
            }
            var majorDtos = _mapper.Map<List<MajorDto>>(result);

            foreach (var majorDto in majorDtos)
            {
                var major = result.FirstOrDefault(c => c.MajorId == majorDto.MajorId);
                majorDto.JobPositionDetails = major.JobPositions
                    .Select(jobPosition => new JobPositionDetailsDto
                    {
                        JobPositionId = jobPosition.JobPositionId,
                        JobPositionName = jobPosition.JobPositionName,
                        JobPositionCode = jobPosition.JobPositionCode,
                        JobPositionDescription = jobPosition.JobPositionDescription,
                        JobPositionPermission = jobPosition.JobPositionPermission
                    }).ToList();
                majorDto.CertificationDetails = major.Certs
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
            return majorDtos;
        }
        public async Task<List<MajorDto>> GetMajorByTwoIdAsync(int majorId, int jobPositionId, CancellationToken cancellationToken)
        {
            var results = await _uow.MajorRepository.WhereAsync(
                x => x.MajorId == majorId,
                cancellationToken: cancellationToken,
                include: query => query.Include(c => c.Certs)
                                       .ThenInclude(cert => cert.JobPositions)
                                       .Include(c => c.Certs)
                                       .ThenInclude(cert => cert.Type)
                                       .Include(c => c.Certs)
                                       .ThenInclude(cert => cert.Organize));

            if (results == null || !results.Any())
            {
                throw new KeyNotFoundException("No majors found.");
            }

            var majorDtoList = results.Select(result =>
            {
                var majorDto = _mapper.Map<MajorDto>(result);

                majorDto.CertificationDetails = result.Certs
                    .Where(cert => cert.Majors.Any(m => m.MajorId == majorId) &&
                                   cert.JobPositions.Any(j => j.JobPositionId == jobPositionId))
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

                return majorDto;
            }).ToList();

            return majorDtoList;
        }


        public async Task<MajorDto> UpdateMajorAsync(int majorId, UpdateMajorRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateMajorValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var major = await _uow.MajorRepository
                .Include(x => x.JobPositions)
                .Include(c => c.Certs)
                .FirstOrDefaultAsync(x => x.MajorId == majorId, cancellationToken)
                .ConfigureAwait(false);
            if (major is null)
            {
                throw new KeyNotFoundException("Major not found.");
            }
            major.MajorCode = request.MajorCode;
            major.MajorName = request.MajorName;
            major.MajorDescription = request.MajorDescription;
            major.MajorImage = request.MajorImage;
            major.MajorPermission = "Pending";

            // Get existing JobPosition IDs
            var existingJobPositionIds = major.JobPositions.Select(j => j.JobPositionId).ToList();
            var newJobPositionIds = request.JobPositionId ?? new List<int>();

            // Remove JobPositions that are no longer referenced
            foreach (var existingJobPositionId in existingJobPositionIds)
            {
                if (!newJobPositionIds.Contains(existingJobPositionId))
                {
                    var jobPositionToRemove = major.JobPositions.FirstOrDefault(j => j.JobPositionId == existingJobPositionId);
                    if (jobPositionToRemove != null)
                    {
                        major.JobPositions.Remove(jobPositionToRemove);
                    }
                }
            }

            // Add new JobPositions that are not already in the Major
            foreach (var newJobPositionId in newJobPositionIds)
            {
                if (!existingJobPositionIds.Contains(newJobPositionId))
                {
                    var jobPosition = await _uow.JobPositionRepository
                        .FirstOrDefaultAsync(x => x.JobPositionId == newJobPositionId, cancellationToken);

                    if (jobPosition != null)
                    {
                        // Check if this relationship already exists to avoid duplicates
                        if (!major.JobPositions.Any(j => j.JobPositionId == newJobPositionId))
                        {
                            major.JobPositions.Add(jobPosition);
                        }
                    }
                    else
                    {
                        throw new KeyNotFoundException($"JobPosition with ID {newJobPositionId} not found.");
                    }
                }
            }
            // Update Certs
            var existingCertIds = new HashSet<int>(major.Certs.Select(x => x.CertId));
            var newCertIds = request.CertId ?? new List<int>();

            foreach (var existingCertId in existingCertIds)
            {
                if (!newCertIds.Contains(existingCertId))
                {
                    var certToRemove = major.Certs.FirstOrDefault(c => c.CertId == existingCertId);
                    if (certToRemove != null)
                    {
                        major.Certs.Remove(certToRemove);
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
                        if (!major.Certs.Any(c => c.CertId == newCertId))
                        {
                            major.Certs.Add(certification);
                        }
                    }
                    else
                    {
                        throw new KeyNotFoundException($"Certification with ID {newCertId} not found.");
                    }
                }
            }

            // Update the Major in the repository
            _uow.MajorRepository.Update(major);

            try
            {
                await _uow.Commit(cancellationToken);

                // Create the DTO and populate JobPosition details
                var majorDto = _mapper.Map<MajorDto>(major);
                majorDto.JobPositionId = major.JobPositions.Select(j => j.JobPositionId).ToList();
                majorDto.CertId = major.Certs.Select(c => c.CertId).ToList();

                return majorDto;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new Exception("The Major you're trying to update has been modified by another user. Please try again.", ex);
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

        public async Task<MajorDto> UpdateMajorPermissionAsync(int majorId, EnumPermission majorPermission, CancellationToken cancellationToken)
        {
            var major = await _uow.MajorRepository.FirstOrDefaultAsync(x => x.MajorId == majorId, cancellationToken);

            if (major is null)
            {
                throw new KeyNotFoundException("Major not found.");
            }

            major.MajorPermission = majorPermission.ToString();

            _uow.MajorRepository.Update(major);
            await _uow.Commit(cancellationToken);

            var result = _mapper.Map<MajorDto>(major);

            return result;
        }
    }
}
