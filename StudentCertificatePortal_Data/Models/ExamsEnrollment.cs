using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class ExamsEnrollment
{
    public int ExamEnrollmentId { get; set; }

    public DateTime? ExamEnrollmentDate { get; set; }

    public string? ExamEnrollmentStatus { get; set; }

    public int? TotalPrice { get; set; }

    public int? UserId { get; set; }

    public int? TotalPriceVoucher { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<StudentOfExam> StudentOfExams { get; set; } = new List<StudentOfExam>();

    public virtual User? User { get; set; }
}
