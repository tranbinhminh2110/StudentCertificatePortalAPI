using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class NotificationDto : IMapFrom<Notification>
    {
        public int NotificationId { get; set; }

        public string? NotificationName { get; set; }

        public string? NotificationDescription { get; set; }

        public string? NotificationImage { get; set; }

        public DateTime? CreationDate { get; set; }

        public string? Role { get; set; }
    }
}
