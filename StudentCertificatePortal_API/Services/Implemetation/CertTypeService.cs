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
    public class CertTypeService: ICertTypeService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateCertTypeRequest> _addCertTypevalidator;
        private readonly IValidator<UpdateCertTypeRequest> _updateCertTypevalidator;


        public CertTypeService(IUnitOfWork uow, IMapper mapper, IValidator<CreateCertTypeRequest> addCertTypevalidator,
            IValidator<UpdateCertTypeRequest> updateCertTypevalidator) {
            _uow = uow;
            _mapper = mapper;
            _addCertTypevalidator = addCertTypevalidator;
            _updateCertTypevalidator = updateCertTypevalidator;
        }

        public async Task<CertTypeDto> CreateCertTypeAsync(CreateCertTypeRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addCertTypevalidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var certType = await _uow.CertTypeRepository.FirstOrDefaultAsync(
                x => x.TypeCode.Equals(request.TypeCode) && x.TypeName.Equals(request.TypeName));

            if(certType != null)
            {
                throw new Exception("The type of Certification is existed.");
            }


            var certTypeEntity = new CertType()
            {
                TypeCode = request.TypeCode,
                TypeName = request.TypeName,
            };
            var result = await _uow.CertTypeRepository.AddAsync(certTypeEntity);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<CertTypeDto>(result);
        }

        public async Task<CertTypeDto> DeleteCertTypeAsync(int certTypeId, CancellationToken cancellationToken)
        {
            var certType = await _uow.CertTypeRepository.FirstOrDefaultAsync(x => x.TypeId == certTypeId,
                cancellationToken,
                include: c => c.Include(p => p.Certifications));

            if (certType == null)
            {
                throw new KeyNotFoundException("The type id of certification not found.");
            }

            certType.Certifications.Clear();

            var result = _uow.CertTypeRepository.Delete(certType);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<CertTypeDto>(result);
        }

        public async Task<List<CertTypeDto>> GetAll()
        {
            var result = await _uow.CertTypeRepository.GetAll();
            return _mapper.Map<List<CertTypeDto>>(result);
        }

        public async Task<CertTypeDto> GetCertTypeByIdAsync(int certTypeId, CancellationToken cancellationToken)
        {
            var certType = await _uow.CertTypeRepository.FirstOrDefaultAsync(x => x.TypeId == certTypeId);

            if (certType == null)
            {
                throw new KeyNotFoundException("The type of cetification not found.");
            }

            return _mapper.Map<CertTypeDto>(certType);
        }
        public async Task<List<CertTypeDto>> GetCertTypeByNameAsync(string certTypeName, CancellationToken cancellationToken)
        {
            var certType = await _uow.CertTypeRepository.WhereAsync(x => x.TypeName.Contains(certTypeName));

            return _mapper.Map<List<CertTypeDto>>(certType.ToList());
        }

        public async Task<CertTypeDto> UpdateCertTypeAsync(int certTypeId, UpdateCertTypeRequest request, CancellationToken cancellationToken)
        {
            var certType = await _uow.CertTypeRepository.FirstOrDefaultAsync(x => x.TypeId == certTypeId);

            if(certType == null)
            {
                throw new KeyNotFoundException("The type of cetification not found.");
            }

            certType.TypeName = request.TypeName;
            certType.TypeCode = request.TypeCode;

            var result = _uow.CertTypeRepository.Update(certType);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<CertTypeDto>(result);
        }
    }
}
