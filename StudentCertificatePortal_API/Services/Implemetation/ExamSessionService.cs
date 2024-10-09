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
    public class ExamSessionService : IExamSessionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateExamSessionRequest> _addExamSessionValidator;
        private readonly IValidator<UpdateExamSessionRequest> _updateExamSessionValidator;

        public ExamSessionService(IUnitOfWork uow, IMapper mapper,IValidator<CreateExamSessionRequest> addExamSessionValidator, IValidator<UpdateExamSessionRequest> updateExamSessionValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _addExamSessionValidator = addExamSessionValidator;
            _updateExamSessionValidator = updateExamSessionValidator;
        }

        public async Task<ExamSessionDto> CreateExamSessionAsync(CreateExamSessionRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addExamSessionValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var certification = await _uow.CertificationRepository.FirstOrDefaultAsync(x => x.CertId == request.CertId, cancellationToken);

            if (certification == null)
            {
                throw new Exception("Certification not found. Course creation requires a valid CertId.");
            }
            var sessionEntity = new ExamSession()
            {
                SessionName = request.SessionName,
                SessionCode = request.SessionCode,
                SessionDate = request.SessionDate,
                SessionAddress = request.SessionAddress,
                CertId = request.CertId,
                SessionCreatedAt = request.SessionCreatedAt,
            };
            var result = await _uow.ExamSessionRepository.AddAsync(sessionEntity);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<ExamSessionDto>(result);
        }

        public async Task<ExamSessionDto> DeleteExamSessionAsync(int sessionId, CancellationToken cancellationToken)
        {
            var session = await _uow.ExamSessionRepository.FirstOrDefaultAsync(x => x.SessionId == sessionId, cancellationToken);
            if(session is null)
            {
                throw new KeyNotFoundException("ExamSession not found.");
            }
            _uow.ExamSessionRepository.Delete(session);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<ExamSessionDto>(session);
        }

        public async Task<List<ExamSessionDto>> GetAll()
        {
            var result = await _uow.ExamSessionRepository.GetAll();
            return _mapper.Map<List<ExamSessionDto>>(result);
        }

        public async Task<ExamSessionDto> GetExamSessionByIdAsync(int sessionId, CancellationToken cancellationToken)
        {
            var result = await _uow.ExamSessionRepository.FirstOrDefaultAsync(x => x.SessionId == sessionId, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("ExamSession not found.");
            }
            return _mapper.Map<ExamSessionDto>(result);
        }

        public async Task<List<ExamSessionDto>> GetExamSessionByNameAsync(string sessionName, CancellationToken cancellationToken)
        {
            var result = await _uow.ExamSessionRepository.WhereAsync(x => x.SessionName.Contains(sessionName), cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("ExamSession not found.");
            }
            return _mapper.Map<List<ExamSessionDto>>(result);
        }

        public async Task<ExamSessionDto> UpdateExamSessionAsync(int sessionId, UpdateExamSessionRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateExamSessionValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var session = await _uow.ExamSessionRepository.FirstOrDefaultAsync(x => x.SessionId == sessionId, cancellationToken);
            if (session is null) 
            {
                throw new KeyNotFoundException("ExamSession not found.");
            }
            session.SessionName = request.SessionName;
            session.SessionCode = request.SessionCode;
            session.SessionDate = request.SessionDate;
            session.SessionAddress = request.SessionAddress;
            _uow.ExamSessionRepository.Update(session);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<ExamSessionDto>(session);
        }
    }
}
