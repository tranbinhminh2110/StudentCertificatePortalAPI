using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class UserAnswer
{
    public int UserAnswerId { get; set; }

    public int? UserId { get; set; }

    public int? ExamId { get; set; }

    public int? QuestionId { get; set; }

    public string? AnswerContent { get; set; }

    public int? AnswerId { get; set; }

    public bool IsCorrect { get; set; }

    public decimal? ScoreValue { get; set; }

    public DateTime SubmittedAt { get; set; }

    public string QuestionType { get; set; } = null!;

    public int? ScoreId { get; set; }

    public virtual SimulationExam? Exam { get; set; }

    public virtual ICollection<PeerReviewDetail> PeerReviewDetails { get; set; } = new List<PeerReviewDetail>();

    public virtual Question? Question { get; set; }

    public virtual Score? Score { get; set; }

    public virtual User? User { get; set; }
}
