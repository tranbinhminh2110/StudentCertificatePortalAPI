using AutoMapper;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class MajorService : IMajorService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public MajorService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<MajorDto> CreateMajorAsync(CreateMajorRequest request, CancellationToken cancellationToken)
        {
            var majorEntity = new Major()
            {
                MajorCode = request.MajorCode,
                MajorName = request.MajorName,
                MajorDescription = request.MajorDescription,
            };
            var result = await _uow.MajorRepository.AddAsync(majorEntity);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<MajorDto>(result);
        }

        public async Task<MajorDto> DeleteMajorAsync(int majorId, CancellationToken cancellationToken)
        {
            var major = await _uow.MajorRepository.FirstOrDefaultAsync(x => x.MajorId == majorId, cancellationToken);
            if (major is null) 
            {
                throw new KeyNotFoundException("Major not found.");
            }
            _uow.MajorRepository.Delete(major);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<MajorDto>(major);
        }

        public async Task<List<MajorDto>> GetAll()
        {
            var result = await _uow.MajorRepository.GetAll();
            return _mapper.Map<List<MajorDto>>(result);
        }

        public async Task<MajorDto> GetMajorByIdAsync(int majorId, CancellationToken cancellationToken)
        {
            var result = await _uow.MajorRepository.FirstOrDefaultAsync(x => x.MajorId == majorId, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Major not found.");
            }
            return _mapper.Map<MajorDto>(result);
        }

        public async Task<MajorDto> UpdateMajorAsync(int majorId, UpdateMajorRequest request, CancellationToken cancellationToken)
        {
            var major = await _uow.MajorRepository.FirstOrDefaultAsync(x => x.MajorId == majorId, cancellationToken);
            if (major is null)
            {
                throw new KeyNotFoundException("Major not found.");
            }
            major.MajorCode = request.MajorCode;
            major.MajorName = request.MajorName;
            major.MajorDescription = request.MajorDescription;

            _uow.MajorRepository.Update(major);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<MajorDto>(major);
        }
    }
}
