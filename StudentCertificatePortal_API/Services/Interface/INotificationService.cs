using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetAll();
        Task<NotificationDto> GetNotificationByIdAsync(int notificationId, CancellationToken cancellationToken);
        Task<List<NotificationDto>> GetNotificationByRoleAsync(string role, CancellationToken cancellationToken);
        Task<NotificationDto> DeleteNotificationAsync(int notificationId, CancellationToken cancellationToken);
        Task<List<NotificationDto>> UpdateNotificationIsReadAsync(string role, CancellationToken cancellationToken);
        Task<List<NotificationDto>> UpdateIsReadByIdAsync(int notificationId, CancellationToken cancellationToken);
        Task<List<NotificationDto>> UpdateAdminIsReadAsync(int notificationId, CancellationToken cancellationToken);
        Task<List<NotificationDto>> GetNotificationByStudentAsync(int userId, CancellationToken cancellationToken);


    }
}
