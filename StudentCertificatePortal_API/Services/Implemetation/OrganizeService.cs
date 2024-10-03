using AutoMapper;
using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class OrganizeService : IOrganizeService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        /*        private readonly IValidator<CreateOrganizeRequest> _addOrganizeValidator;
                private readonly IValidator<UpdateOrganizeRequest> _updateOrganizeValidator;*/
        public OrganizeService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<OrganizeDto> CreateOrganizeAsync(CreateOrganizeRequest request, CancellationToken cancellationToken)
        {
            var organizeEntity = new Organize()
            {
                OrganizeName = request.OrganizeName,
                OrganizeAddress = request.OrganizeAddress?.Trim(),
                OrganizeContact = request.OrganizeContact,
            };
            var result = await _uow.OrganizeRepository.AddAsync(organizeEntity);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<OrganizeDto>(result);
        }

        public async Task<OrganizeDto> DeleteOrganizeAsync(int organizeId, CancellationToken cancellationToken)
        {
            var organize = await _uow.OrganizeRepository.FirstOrDefaultAsync(x => x.OrganizeId == organizeId, cancellationToken);
            if (organize is null)
            {
                throw new KeyNotFoundException("Organize not found.");
            }
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

        public async Task<OrganizeDto> UpdateOrganizeAsync(int oragnizeId, UpdateOrganizeRequest request, CancellationToken cancellationToken)
        {
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
    }
}
