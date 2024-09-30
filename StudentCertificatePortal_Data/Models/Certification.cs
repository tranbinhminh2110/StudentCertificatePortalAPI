using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Certification
{
    public int CertId { get; set; }

    public string? CertName { get; set; }

    public string? CertCode { get; set; }

    public string? CertDescription { get; set; }

    public int? CertCost { get; set; }

    public string? CertPointSystem { get; set; }

    public string? CertImage { get; set; }

    public string? CertPrerequisite { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public int? TypeId { get; set; }

    public int? OrganizeId { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<ExamSession> ExamSessions { get; set; } = new List<ExamSession>();

    public virtual ICollection<JobCert> JobCerts { get; set; } = new List<JobCert>();

    public virtual Organize? Organize { get; set; }

    public virtual ICollection<SimulationExam> SimulationExams { get; set; } = new List<SimulationExam>();

    public virtual CertType? Type { get; set; }

    public virtual ICollection<Certification> CertIdTwos { get; set; } = new List<Certification>();

    public virtual ICollection<Certification> Certs { get; set; } = new List<Certification>();
}
