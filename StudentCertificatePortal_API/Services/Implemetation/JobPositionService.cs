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
    public class JobPositionService : IJobPositionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateJobPositionRequest> _addJobPositonValidator;
        private readonly IValidator<UpdateJobPositionRequest> _updateJobPositonValidator;

        public JobPositionService(IUnitOfWork uow, IMapper mapper, IValidator<CreateJobPositionRequest> addJobPositionValidator, IValidator<UpdateJobPositionRequest> updateJobPositionValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _addJobPositonValidator = addJobPositionValidator;
            _updateJobPositonValidator = updateJobPositionValidator;
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
                JobPositionCode = request.JobPositionCode,
                JobPositionName = request.JobPositionName,
                JobPositionDescription = request.JobPositionDescription,
            };
            var result = await _uow.JobPositionRepository.AddAsync(jobEntity);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<JobPositionDto>(result);
        }

        public async Task<JobPositionDto> DeleteJobPositionAsync(int jobPositionId, CancellationToken cancellationToken)
        {
            var job = await _uow.JobPositionRepository.FirstOrDefaultAsync(x => x.JobPositionId == jobPositionId, cancellationToken);
            if (job is null)
            {
                throw new KeyNotFoundException("JobPosition not found.");
            }
            _uow.JobPositionRepository.Delete(job);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<JobPositionDto>(job);
        }

        public async Task<List<JobPositionDto>> GetAll()
        {
            var result = await _uow.JobPositionRepository.GetAll();
            return _mapper.Map<List<JobPositionDto>>(result);
        }

        public async Task<JobPositionDto> GetJobPositionByIdAsync(int jobPositionId, CancellationToken cancellationToken)
        {
            var result = await _uow.JobPositionRepository.FirstOrDefaultAsync(x => x.JobPositionId == jobPositionId, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("JobPosition not found.");
            }
            return _mapper.Map<JobPositionDto>(result);
        }

        public async Task<List<JobPositionDto>> GetJobPositionByNameAsync(string jobPositionName, CancellationToken cancellationToken)
        {
            var result = await _uow.JobPositionRepository.WhereAsync(x => x.JobPositionName.Contains(jobPositionName), cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Major not found.");
            }
            return _mapper.Map<List<JobPositionDto>>(result);
        }

        public async Task<JobPositionDto> UpdateJobPositionAsync(int jobPositionId, UpdateJobPositionRequest request, CancellationToken cancellationToken = default)
        {
            var validation = await _updateJobPositonValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var job = await _uow.JobPositionRepository.FirstOrDefaultAsync(x => x.JobPositionId == jobPositionId, cancellationToken);
            if (job is null)
            {
                throw new KeyNotFoundException("JobPosition not found.");
            }
            job.JobPositionCode = request.JobPositionCode;
            job.JobPositionName = request.JobPositionName;
            job.JobPositionDescription = request.JobPositionDescription;

            _uow.JobPositionRepository.Update(job);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<JobPositionDto>(job);
        }
    }
}
