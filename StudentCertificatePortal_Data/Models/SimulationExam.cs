using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class SimulationExam
{
    public int ExamId { get; set; }

    public string? ExamName { get; set; }

    public string? ExamCode { get; set; }

    public int? CertId { get; set; }

    public string? ExamDescription { get; set; }

    public int? ExamFee { get; set; }

    public int? ExamDiscountFee { get; set; }

    public string? ExamImage { get; set; }

    public string? ExamPermission { get; set; }

    public virtual Certification? Cert { get; set; }

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();

    public virtual ICollection<StudentOfExam> StudentOfExams { get; set; } = new List<StudentOfExam>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
}
