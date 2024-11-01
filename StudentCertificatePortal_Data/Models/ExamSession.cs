using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class ExamSession
{
    public int SessionId { get; set; }

    public string? SessionName { get; set; }

    public string? SessionCode { get; set; }

    public DateTime? SessionDate { get; set; }

    public string? SessionAddress { get; set; }

    public int? CertId { get; set; }

    public DateTime? SessionCreatedAt { get; set; }

    public string? SessionTime { get; set; }

    public virtual Certification? Cert { get; set; }
}
