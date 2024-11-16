using AutoMapper;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public NotificationService(IUnitOfWork uow, IMapper mapper) 
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<NotificationDto> DeleteNotificationAsync(int notificationId, CancellationToken cancellationToken)
        {
            var noti = await _uow.NotificationRepository.FirstOrDefaultAsync(
                x => x.NotificationId == notificationId, cancellationToken);
            if (noti is null)
            {
                throw new KeyNotFoundException("Notification not found.");

            }
            _uow.NotificationRepository.Delete(noti);
            await _uow.Commit(cancellationToken);
            var notificationDto = _mapper.Map<NotificationDto>(noti);
            return notificationDto;
        }

        public async Task<List<NotificationDto>> GetAll()
        {
            var result = await _uow.NotificationRepository.GetAll();
            var sortedResult = result.OrderByDescending(x => x.CreationDate);
            return _mapper.Map<List<NotificationDto>>(sortedResult);
        }

        public async Task<NotificationDto> GetNotificationByIdAsync(int notificationId, CancellationToken cancellationToken)
        {
            var result = await _uow.NotificationRepository.FirstOrDefaultAsync(x => x.NotificationId == notificationId, cancellationToken);
            if(result is null)
            {
                throw new KeyNotFoundException("Notification not found.");
            }
            return _mapper.Map<NotificationDto>(result);
        }

        public async Task<List<NotificationDto>> GetNotificationByRoleAsync(string role, CancellationToken cancellationToken)
        {
            var result = await _uow.NotificationRepository.WhereAsync(x => x.Role == role, cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Notification not found.");
            }
            var sortedResult = result.OrderByDescending(x => x.CreationDate); 
            return _mapper.Map<List<NotificationDto>>(sortedResult);
        }

        public async Task<List<NotificationDto>> UpdateNotificationIsReadAsync(string role, CancellationToken cancellationToken)
        {
            var notifications = await _uow.NotificationRepository.WhereAsync(
                x => x.Role == role, cancellationToken);
            if (notifications == null || !notifications.Any())
            {
                throw new KeyNotFoundException("No unread notifications found for this role.");
            }
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            await _uow.Commit(cancellationToken);
            var sortedResult = notifications.OrderByDescending(x => x.CreationDate); 
            return _mapper.Map<List<NotificationDto>>(sortedResult);

        }

        public async Task<List<NotificationDto>> UpdateAdminIsReadAsync(string role, CancellationToken cancellationToken)
        {
            var notifications = await _uow.NotificationRepository.WhereAsync(
                x => x.Role == role && x.IsRead == false, cancellationToken);

            if (notifications == null || !notifications.Any())
            {
                throw new KeyNotFoundException("There are no unread notifications for this role.");
            }

            foreach (var notification in notifications)
            {
                notification.IsRead = true;

                if (notification.NotificationName == "Feedback contains forbidden words")
                {
                    var userId = notification.UserId;

                    var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
                    if (user != null)
                    {
                        user.UserOffenseCount++;

                        var studentNotification = new Notification()
                        {
                            NotificationName = "Feedback flagged for review",
                            NotificationDescription = $"Your feedback has been flagged for containing inappropriate language. Please ensure compliance with community guidelines to avoid further issues.",
                            NotificationImage = notification.NotificationImage,
                            CreationDate = DateTime.UtcNow,
                            Role = "Student",
                            IsRead = false,
                            UserId = userId,
                        };

                        await _uow.NotificationRepository.AddAsync(studentNotification);

                        _uow.UserRepository.Update(user);
                    }
                }
            }

            await _uow.Commit(cancellationToken);

            var sortedResult = notifications.OrderByDescending(x => x.CreationDate);
            return _mapper.Map<List<NotificationDto>>(sortedResult);
        }


        public async Task<List<NotificationDto>> GetNotificationByStudentAsync(int userId, CancellationToken cancellationToken)
        {
            var result = await _uow.NotificationRepository.WhereAsync(x => x.UserId == userId && x.Role == "Student", cancellationToken);
            if (result is null)
            {
                throw new KeyNotFoundException("Notification not found.");
            }
            var sortedResult = result.OrderByDescending(x => x.CreationDate);
            return _mapper.Map<List<NotificationDto>>(sortedResult);
        }

    }
}
 