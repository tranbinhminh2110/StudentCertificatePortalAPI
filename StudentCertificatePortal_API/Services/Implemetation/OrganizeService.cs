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

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class OrganizeService : IOrganizeService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateOrganizeRequest> _addOrganizeValidator;
        private readonly IValidator<UpdateOrganizeRequest> _updateOrganizeValidator;
        public OrganizeService(IUnitOfWork uow, IMapper mapper, IValidator<CreateOrganizeRequest> addOrganizeValidator, IValidator<UpdateOrganizeRequest> updateOrganizeValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _addOrganizeValidator = addOrganizeValidator;
            _updateOrganizeValidator = updateOrganizeValidator;
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
            var result = await _uow.OrganizeRepository.GetAll();
            return _mapper.Map<List<OrganizeDto>>(result);
        }

        public async Task<OrganizeDto> GetOrganizeByIdAsync(int organizeId, CancellationToken cancellationToken)
        {
            var result = await _uow.OrganizeRepository.FirstOrDefaultAsync(x => x.OrganizeId == organizeId, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Organize not found.");
            }
            return _mapper.Map<OrganizeDto>(result);
        }

        public async Task<List<OrganizeDto>> GetOrganizeByNameAsync(string organizeName, CancellationToken cancellationToken)
        {
            var result = await _uow.OrganizeRepository.WhereAsync(x => x.OrganizeName.Contains(organizeName), cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Organize not found.");
            }
            return _mapper.Map<List<OrganizeDto>>(result);
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

            _uow.OrganizeRepository.Update(organize);
            await _uow.Commit(cancellationToken);

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

            var result = _mapper.Map<OrganizeDto>(organize);

            return result;
        }
    }
}
