using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Score
{
    public int ScoreId { get; set; }

    public int? UserId { get; set; }

    public int? ExamId { get; set; }

    public decimal? ScoreValue { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual SimulationExam? Exam { get; set; }

    public virtual ICollection<PeerReview> PeerReviews { get; set; } = new List<PeerReview>();

    public virtual User? User { get; set; }

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
}
