using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public DateTime? PaymentDate { get; set; }

    public int? PaymentPoint { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentStatus { get; set; }

    public int? WalletId { get; set; }

    public int? ExamEnrollmentId { get; set; }

    public int? CourseEnrollmentId { get; set; }

    public virtual CoursesEnrollment? CourseEnrollment { get; set; }

    public virtual ExamsEnrollment? ExamEnrollment { get; set; }

    public virtual Wallet? Wallet { get; set; }
}
