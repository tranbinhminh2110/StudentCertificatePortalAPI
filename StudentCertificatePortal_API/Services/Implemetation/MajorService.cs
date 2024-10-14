using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

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
            await _uow.MajorRepository.AddAsync(majorEntity);
            try
            {
                await _uow.Commit(cancellationToken);
                var majorDto = _mapper.Map<MajorDto>(majorEntity);
                majorDto.JobPositionId = majorEntity.JobPositions
                    .Select(majorjob => majorjob.JobPositionId)
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
                cancellationToken, include: q => q.Include(c => c.JobPositions));
            if (major is null) 
            {
                throw new KeyNotFoundException("Major not found.");
            }
            major.JobPositions?.Clear();

            _uow.MajorRepository.Delete(major);
            await _uow.Commit(cancellationToken);

            var majorDto = _mapper.Map<MajorDto>(major);
            return majorDto;
        }

        public async Task<List<MajorDto>> GetAll()
        {
            var result = await _uow.MajorRepository.GetAllAsync(query => 
            query.Include(c => c.JobPositions));

            var majorDtos = result.Select(result =>
            {
                var majorDto = _mapper.Map<MajorDto>(result);

                majorDto.JobPositionName = result.JobPositions
                .Select(majorjob => majorjob.JobPositionName)
                .ToList();
                majorDto.JobPositionCode = result.JobPositions
                .Select (majorjob => majorjob.JobPositionCode) 
                .ToList();
                majorDto.JobPositionDescription = result.JobPositions
                .Select(majorjob => majorjob.JobPositionDescription)
                .ToList();

                return majorDto;
            }).ToList();
            return majorDtos;
        }

        public async Task<MajorDto> GetMajorByIdAsync(int majorId, CancellationToken cancellationToken)
        {
            var result = await _uow.MajorRepository.FirstOrDefaultAsync(
                x => x.MajorId == majorId, cancellationToken: cancellationToken,include: query => query.Include(c => c.JobPositions));
            if (result is null)
            {
                throw new KeyNotFoundException("Major not found.");
            }
            var majorDto = _mapper.Map<MajorDto>(result);

            majorDto.JobPositionName = result.JobPositions
            .Select(majorjob => majorjob.JobPositionName)
            .ToList();
            majorDto.JobPositionCode = result.JobPositions
                .Select(majorjob => majorjob.JobPositionCode)
                .ToList();
            majorDto.JobPositionDescription = result.JobPositions
            .Select(majorjob => majorjob.JobPositionDescription)
            .ToList();

            return majorDto;
        }

        public async Task<List<MajorDto>> GetMajorByNameAsync(string majorName, CancellationToken cancellationToken)
        {
            var result = await _uow.MajorRepository.WhereAsync(x => x.MajorName.Contains(majorName), cancellationToken,
                    include: query => query.Include(c => c.JobPositions));
            if (result is null || !result.Any())
            {
                throw new KeyNotFoundException("Major not found.");
            }
            var majorDtos = _mapper.Map<List<MajorDto>>(result);

            foreach (var majorDto in majorDtos)
            {
                var major = result.FirstOrDefault(c => c.MajorId == majorDto.MajorId);
                majorDto.JobPositionName = major.JobPositions
                .Select(majorjob => majorjob.JobPositionName)
                .ToList(); 
                majorDto.JobPositionCode = major.JobPositions
                .Select(majorjob => majorjob.JobPositionCode)
                .ToList();  
                majorDto.JobPositionDescription = major.JobPositions
                .Select(majorjob => majorjob.JobPositionDescription)
                .ToList();
            }
            return majorDtos;
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
                .FirstOrDefaultAsync(x => x.MajorId == majorId, cancellationToken)
                .ConfigureAwait(false);
            if (major is null)
            {
                throw new KeyNotFoundException("Major not found.");
            }
            major.MajorCode = request.MajorCode;
            major.MajorName = request.MajorName;
            major.MajorDescription = request.MajorDescription;

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

            // Update the Major in the repository
            _uow.MajorRepository.Update(major);

            try
            {
                await _uow.Commit(cancellationToken);

                // Create the DTO and populate JobPosition details
                var majorDto = _mapper.Map<MajorDto>(major);
                majorDto.JobPositionId = major.JobPositions.Select(j => j.JobPositionId).ToList();

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
    }
}
