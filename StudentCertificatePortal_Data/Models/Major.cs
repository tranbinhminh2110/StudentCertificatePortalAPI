using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Major
{
    public int MajorId { get; set; }

    public string? MajorCode { get; set; }

    public string? MajorName { get; set; }

    public string? MajorDescription { get; set; }

    public virtual ICollection<Certification> Certs { get; set; } = new List<Certification>();

    public virtual ICollection<JobPosition> JobPositions { get; set; } = new List<JobPosition>();
}
