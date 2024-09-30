using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class JobCert
{
    public int CertId { get; set; }

    public int JobPositionId { get; set; }

    public virtual Certification Cert { get; set; } = null!;

    public virtual JobPosition CertNavigation { get; set; } = null!;
}
