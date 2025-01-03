﻿using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Certification
{
    public int CertId { get; set; }

    public string? CertName { get; set; }

    public string? CertCode { get; set; }

    public int? CertCost { get; set; }

    public string? CertImage { get; set; }

    public int? TypeId { get; set; }

    public int? OrganizeId { get; set; }

    public string? CertValidity { get; set; }

    public string? CertDescription { get; set; }

    public string? Permission { get; set; }

    public string? CertPointSystem { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<ExamSession> ExamSessions { get; set; } = new List<ExamSession>();

    public virtual Organize? Organize { get; set; }

    public virtual ICollection<SimulationExam> SimulationExams { get; set; } = new List<SimulationExam>();

    public virtual CertType? Type { get; set; }

    public virtual ICollection<Certification> CertIdPrerequisites { get; set; } = new List<Certification>();

    public virtual ICollection<Certification> Certs { get; set; } = new List<Certification>();

    public virtual ICollection<JobPosition> JobPositions { get; set; } = new List<JobPosition>();

    public virtual ICollection<Major> Majors { get; set; } = new List<Major>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
