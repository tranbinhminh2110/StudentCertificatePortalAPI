using AutoMapper;
using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Validators;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        private readonly IValidator<CreateFeedbackRequest> _addFeedbackValidator;
        private readonly IValidator<UpdateFeedbackRequest> _updateFeedbackValidator;

        public FeedbackService(IUnitOfWork uow, IMapper mapper, IValidator<CreateFeedbackRequest> addFeedbackValidator, IValidator<UpdateFeedbackRequest> updateFeedbackValidator)
        {
            _uow = uow;
            _mapper = mapper;
            _addFeedbackValidator = addFeedbackValidator;
            _updateFeedbackValidator = updateFeedbackValidator;
        }

        public async Task<FeedbackDto> CreateFeedbackAsync(CreateFeedbackRequest request, CancellationToken cancellationToken)
        {
            var validation = await _addFeedbackValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);
            if (user is null)
            {
                throw new KeyNotFoundException("User not found.");
            }
            /*var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.SimulationExamId == request.SimulationExamId, cancellationToken);
            if (exam is null) 
            {
                throw new KeyNotFoundException("SimulationExam not found.");
            }*/
            var feedbackEntity = new Feedback()
            {
                UserId = request.UserId,
                ExamId = request.ExamId,
                FeedbackDescription = request.FeedbackDescription,
                FeedbackImage = request.FeedbackImage,
                FeedbackCreatedAt = request.FeedbackCreatedAt,
            };
            var result = await _uow.FeedbackRepository.AddAsync(feedbackEntity);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<FeedbackDto>(result);
            
        }

        public async Task<FeedbackDto> DeleteFeedbackAsync(int feedbackId, CancellationToken cancellationToken)
        {
            var result = await _uow.FeedbackRepository.FirstOrDefaultAsync(x => x.FeedbackId  == feedbackId, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Feedback not found.");
            }
            _uow.FeedbackRepository.Delete(result);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<FeedbackDto>(result);
        }

        public async Task<List<FeedbackDto>> GetAll()
        {
            var result = await _uow.FeedbackRepository.GetAll();
            return _mapper.Map<List<FeedbackDto>>(result);
        }

        public async Task<List<FeedbackDto>> GetFeedbackByExamIdAsync(int examId, CancellationToken cancellationToken)
        {
            var result = await _uow.FeedbackRepository.WhereAsync(x => x.ExamId == examId, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Feedback not found.");
            }
            return _mapper.Map<List<FeedbackDto>>(result);
        }

        public async Task<FeedbackDto> GetFeedbackByIdAsync(int feedbackId, CancellationToken cancellationToken)
        {
            var result = await _uow.FeedbackRepository.FirstOrDefaultAsync(x => x.FeedbackId == feedbackId, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Feedback not found.");
            }
            return _mapper.Map<FeedbackDto>(result);
        }

        public async Task<List<FeedbackDto>> GetFeedbackByUserIdAsync(int userId, CancellationToken cancellationToken)
        {
            var result = await _uow.FeedbackRepository.WhereAsync(x => x.UserId == userId, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Feedback not found.");
            }
            return _mapper.Map<List<FeedbackDto>>(result);
        }

        public async Task<FeedbackDto> UpdateFeedbackAsync(int feedbackId, UpdateFeedbackRequest request, CancellationToken cancellationToken)
        {
            var validation = await _updateFeedbackValidator.ValidateAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                throw new RequestValidationException(validation.Errors);
            }
            var feedback = await _uow.FeedbackRepository.FirstOrDefaultAsync(x => x.FeedbackId == feedbackId, cancellationToken);
            if (feedback is null)
            {
                throw new KeyNotFoundException("Feedback not found.");
            }
            feedback.FeedbackDescription = request.FeedbackDescription;
            feedback.FeedbackImage = request.FeedbackImage;

            _uow.FeedbackRepository.Update(feedback);
            await _uow.Commit(cancellationToken);
            return _mapper.Map<FeedbackDto>(feedback);
        }
    }
}
