using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Organize
{
    public int OrganizeId { get; set; }

    public string? OrganizeName { get; set; }

    public string? OrganizeAddress { get; set; }

    public string? OrganizeContact { get; set; }

    public virtual ICollection<Certification> Certifications { get; set; } = new List<Certification>();
}
