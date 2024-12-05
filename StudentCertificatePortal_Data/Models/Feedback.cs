using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Feedback
{
    public int FeedbackId { get; set; }

    public string? FeedbackDescription { get; set; }

    public int? UserId { get; set; }

    public int? ExamId { get; set; }

    public DateTime? FeedbackCreatedAt { get; set; }

    public string? FeedbackImage { get; set; }

    public bool? FeedbackPermission { get; set; }

    public int? FeedbackRatingvalue { get; set; }

    public virtual SimulationExam? Exam { get; set; }

    public virtual User? User { get; set; }
}
