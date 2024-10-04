using AutoMapper;
using FluentValidation;
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
                CertPrerequisite = request.CertPrerequisite,
                ExpiryDate = request.ExpiryDate,
            };

            var result = await _uow.CertificationRepository.AddAsync(certificationEntity);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<CertificationDto>(result);
        }

        public async Task<CertificationDto> DeleteCertificationAsync(int certificationId, CancellationToken cancellationToken)
        {
            var certification = await _uow.CertificationRepository.FirstOrDefaultAsync(x => x.CertId == certificationId, cancellationToken);
            if (certification is null)
            {
                throw new KeyNotFoundException("Certification not found.");
            }
            _uow.CertificationRepository.Delete(certification);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<CertificationDto>(certification);
        }

        public async Task<List<CertificationDto>> GetAll()
        {
            var result = await _uow.CertificationRepository.GetAll();
            return _mapper.Map<List<CertificationDto>>(result);
        }

        public async Task<CertificationDto> GetCertificationById(int certificationId, CancellationToken cancellationToken)
        {
            var result = await _uow.CertificationRepository.FirstOrDefaultAsync(x => x.CertId == certificationId, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Certification not found.");
            }
            return _mapper.Map<CertificationDto>(result);
        }

        public async Task<CertificationDto> UpdateCertificationAsync(int certificationId, UpdateCertificationRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateCertificationValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var certification = await _uow.CertificationRepository.FirstOrDefaultAsync(x => x.CertId == certificationId, cancellationToken).ConfigureAwait(false);
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
            certification.CertPrerequisite = request.CertPrerequisite;
            certification.ExpiryDate = request.ExpiryDate;

            _uow.CertificationRepository.Update(certification);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<CertificationDto>(certification);
        }
    }
}
