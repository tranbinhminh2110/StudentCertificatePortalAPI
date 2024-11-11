using AutoMapper;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
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
            return _mapper.Map<List<NotificationDto>>(result);
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
            return _mapper.Map<List<NotificationDto>>(result);
        }
    }
}
