using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class StudentOfExam
{
    public int EnrollmentId { get; set; }

    public int ExamId { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public int? Price { get; set; }

    public bool? Status { get; set; }

    public virtual ExamsEnrollment Enrollment { get; set; } = null!;

    public virtual SimulationExam Exam { get; set; } = null!;
}
