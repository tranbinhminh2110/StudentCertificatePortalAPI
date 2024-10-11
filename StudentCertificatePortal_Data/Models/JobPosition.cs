using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class JobPosition
{
    public int JobPositionId { get; set; }

    public string? JobPositionCode { get; set; }

    public string? JobPositionName { get; set; }

    public string? JobPositionDescription { get; set; }

    public virtual ICollection<Certification> Certs { get; set; } = new List<Certification>();

    public virtual ICollection<Major> Majors { get; set; } = new List<Major>();
}
