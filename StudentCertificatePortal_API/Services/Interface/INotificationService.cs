using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetAll();
        Task<NotificationDto> GetNotificationByIdAsync(int notificationId, CancellationToken cancellationToken);
        Task<List<NotificationDto>> GetNotificationByRoleAsync(string role, CancellationToken cancellationToken);
        Task<NotificationDto> DeleteNotificationAsync(int notificationId, CancellationToken cancellationToken);

    }
}
