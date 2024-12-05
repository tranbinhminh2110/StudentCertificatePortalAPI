using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Exceptions;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;
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

        private readonly ForbiddenWordsService _forbiddenWordsService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;

        public FeedbackService(IUnitOfWork uow, IMapper mapper, IValidator<CreateFeedbackRequest> addFeedbackValidator, IValidator<UpdateFeedbackRequest> updateFeedbackValidator, 
            ForbiddenWordsService forbiddenWordsService, IHubContext<NotificationHub> hubContext, INotificationService notificationService)
        {
            _uow = uow;
            _mapper = mapper;
            _addFeedbackValidator = addFeedbackValidator;
            _updateFeedbackValidator = updateFeedbackValidator;
            _forbiddenWordsService = forbiddenWordsService;
            _hubContext = hubContext;
            _notificationService = notificationService;
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

            var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == request.ExamId, cancellationToken);
            if (exam is null)
            {
                throw new KeyNotFoundException("SimulationExam not found.");
            }
            request.FeedbackRatingvalue ??= 0;

            bool containsForbiddenWord = _forbiddenWordsService.ContainsForbiddenWords(request.FeedbackDescription);

            var feedbackPermission = containsForbiddenWord ? false : true;

            var feedbackEntity = new Feedback()
            {
                UserId = request.UserId,
                ExamId = request.ExamId,
                FeedbackRatingvalue = request.FeedbackRatingvalue,
                FeedbackDescription = request.FeedbackDescription,
                FeedbackImage = request.FeedbackImage,
                FeedbackCreatedAt = request.FeedbackCreatedAt,
                FeedbackPermission = feedbackPermission, 

            };
            var result = await _uow.FeedbackRepository.AddAsync(feedbackEntity);
            await _uow.Commit(cancellationToken);
            if (containsForbiddenWord)
            {
                var notification = new Notification()
                {
                    NotificationName = "Feedback contains forbidden words",
                    NotificationDescription = $"Feedback submitted by user '{user.Username}' for the exam '{exam.ExamName}' contains forbidden words and has been flagged for review. Feedback details: '{request.FeedbackDescription}'.",
                    NotificationImage = user.UserImage,
                    CreationDate = DateTime.UtcNow,
                    Role = "Admin",
                    IsRead = false,
                    UserId = request.UserId,
                };

                await _uow.NotificationRepository.AddAsync(notification);
                await _uow.Commit(cancellationToken);

                var notifications = await _notificationService.GetNotificationByRoleAsync("Admin", cancellationToken);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);
            }
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
            var result = await _uow.FeedbackRepository.GetAllAsync(query =>
                query.Include(f => f.User).Include(f => f.Exam));

            var feedbackDtos = result.Select(feedback =>
            {
                var feedbackDto = _mapper.Map<FeedbackDto>(feedback);
                
                // Ánh xạ thông tin UserDetailsDto vào FeedbackDto
                feedbackDto.UserDetails = feedback.User != null ? new UserDetailsDto
                {
                    UserId = feedback.User.UserId,
                    Username = feedback.User.Username,
                    UserImage = feedback.User.UserImage
                } : null;
                feedbackDto.ExamPermission = feedback.Exam?.ExamPermission;

                return feedbackDto;
            }).ToList();

            return feedbackDtos;
        }

        public async Task<List<FeedbackDto>> GetFeedbackByExamIdAsync(int examId, CancellationToken cancellationToken)
        {
            var result = await _uow.FeedbackRepository.WhereAsync(
                x => x.ExamId == examId,
                cancellationToken: cancellationToken,
                include: query => query.Include(f => f.User)
                .Include(f => f.Exam));

            if (result == null || !result.Any())
            {
                throw new KeyNotFoundException("Feedback not found.");
            }

            var feedbackDtos = result.Select(feedback =>
            {
                var feedbackDto = _mapper.Map<FeedbackDto>(feedback);
                feedbackDto.UserDetails = feedback.User != null ? new UserDetailsDto
                {
                    UserId = feedback.User.UserId,
                    Username = feedback.User.Username,
                    UserImage = feedback.User.UserImage
                } : null;
                feedbackDto.ExamPermission = feedback.Exam?.ExamPermission;
                return feedbackDto;
            }).ToList();

            return feedbackDtos;
        }

        public async Task<FeedbackDto> GetFeedbackByIdAsync(int feedbackId, CancellationToken cancellationToken)
        {
            var result = await _uow.FeedbackRepository.FirstOrDefaultAsync(
                x => x.FeedbackId == feedbackId,
                cancellationToken: cancellationToken,
                include: query => query.Include(f => f.User).Include(f => f.Exam));

            if (result == null)
            {
                throw new KeyNotFoundException("Feedback not found.");
            }

            var feedbackDto = _mapper.Map<FeedbackDto>(result);

            feedbackDto.UserDetails = result.User != null ? new UserDetailsDto
            {
                UserId = result.User.UserId,
                Username = result.User.Username,
                UserImage = result.User.UserImage
            } : null;
            feedbackDto.ExamPermission = result.Exam?.ExamPermission;

            return feedbackDto;
        }

        public async Task<List<FeedbackDto>> GetFeedbackByUserIdAsync(int userId, CancellationToken cancellationToken)
        {
            var result = await _uow.FeedbackRepository.WhereAsync(
                x => x.UserId == userId,
                cancellationToken: cancellationToken,
                include: query => query.Include(f => f.User).Include(f => f.Exam));

            if (result == null || !result.Any())
            {
                throw new KeyNotFoundException("Feedback not found.");
            }

            var feedbackDtos = result.Select(feedback =>
            {
                var feedbackDto = _mapper.Map<FeedbackDto>(feedback);
                feedbackDto.UserDetails = feedback.User != null ? new UserDetailsDto
                {
                    UserId = feedback.User.UserId,
                    Username = feedback.User.Username,
                    UserImage = feedback.User.UserImage
                } : null;
                feedbackDto.ExamPermission = feedback.Exam?.ExamPermission;
                return feedbackDto;
            }).ToList();

            return feedbackDtos;
        }
        public async Task<List<FeedbackDto>> GetFeedbackByCertIdAsync(int certId, CancellationToken cancellationToken)
        {
            var result = await _uow.FeedbackRepository.WhereAsync(
                x => x.Exam != null && x.Exam.CertId == certId, cancellationToken,
                include: query => query.Include(f => f.Exam)
                                       .ThenInclude(e => e.Cert)
                                       .Include(f => f.User) 
            );

            if (result == null || !result.Any())
            {
                throw new KeyNotFoundException("No feedback found for the specified certification.");
            }

            var feedbackDtos = result.Select(feedback =>
            {
                var feedbackDto = _mapper.Map<FeedbackDto>(feedback);
                feedbackDto.UserDetails = new UserDetailsDto
                {
                    UserId = feedback.User.UserId,
                    Username = feedback.User.Username,
                    UserImage = feedback.User.UserImage
                };
                feedbackDto.ExamPermission = feedback.Exam?.ExamPermission;
                return feedbackDto;
            }).ToList();

            return feedbackDtos;
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
            if (request.FeedbackRatingvalue == null || request.FeedbackRatingvalue == 0)
            {
                request.FeedbackRatingvalue = feedback.FeedbackRatingvalue;
            }
            bool containsForbiddenWord = _forbiddenWordsService.ContainsForbiddenWords(request.FeedbackDescription);
            feedback.FeedbackPermission = !containsForbiddenWord;
            feedback.FeedbackRatingvalue = request.FeedbackRatingvalue.Value;
            feedback.FeedbackDescription = request.FeedbackDescription;
            feedback.FeedbackImage = request.FeedbackImage;

            _uow.FeedbackRepository.Update(feedback);
            await _uow.Commit(cancellationToken);
            if (containsForbiddenWord)
            {
                var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == feedback.UserId, cancellationToken);
                if (user == null)
                {
                    throw new KeyNotFoundException("User not found.");
                }

                var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == feedback.ExamId, cancellationToken);
                if (exam == null)
                {
                    throw new KeyNotFoundException("SimulationExam not found.");
                }

                var notification = new Notification()
                {
                    NotificationName = "Feedback contains forbidden words",
                    NotificationDescription = $"Updated feedback by user '{user.Username}' for the exam '{exam.ExamName}' contains forbidden words and has been flagged for review. Feedback details: '{request.FeedbackDescription}'.",
                    NotificationImage = user.UserImage,
                    CreationDate = DateTime.UtcNow,
                    Role = "Admin",
                    IsRead = false,
                    UserId = feedback.UserId,
                };

                await _uow.NotificationRepository.AddAsync(notification);
                await _uow.Commit(cancellationToken);

                // Send notification to admin
                var notifications = await _notificationService.GetNotificationByRoleAsync("Admin", cancellationToken);
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);
            }
            return _mapper.Map<FeedbackDto>(feedback);
        }
        public async Task<(double averageRating, int feedbackCount)> CalculateAverageFeedbackRatingAsync(int simulationExamId, CancellationToken cancellationToken)
        {
            var feedbacks = await _uow.FeedbackRepository.WhereAsync(
                x => x.ExamId == simulationExamId &&
                     x.FeedbackRatingvalue >= 1 && x.FeedbackRatingvalue <= 5 &&
                     x.FeedbackPermission == true, 
                cancellationToken: cancellationToken);

            if (feedbacks == null || !feedbacks.Any())
            {
                throw new KeyNotFoundException("No valid feedback found for this exam.");
            }

            var averageRating = feedbacks.Average(f => f.FeedbackRatingvalue) ?? 0.0;
            var feedbackCount = feedbacks.Count(); 

            return (averageRating, feedbackCount);
        }

    }
}
