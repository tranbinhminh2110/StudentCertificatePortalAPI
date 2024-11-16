using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public string? NotificationName { get; set; }

    public string? NotificationDescription { get; set; }

    public string? NotificationImage { get; set; }

    public DateTime? CreationDate { get; set; }

    public string? Role { get; set; }

    public bool? IsRead { get; set; }

    public int? UserId { get; set; }

    public virtual User? User { get; set; }
}
