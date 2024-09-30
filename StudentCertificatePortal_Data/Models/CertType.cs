using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class CertType
{
    public int TypeId { get; set; }

    public string? TypeCode { get; set; }

    public string? TypeName { get; set; }

    public virtual ICollection<Certification> Certifications { get; set; } = new List<Certification>();
}
