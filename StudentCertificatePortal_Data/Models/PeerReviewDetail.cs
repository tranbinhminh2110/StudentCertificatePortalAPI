using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class PeerReviewDetail
{
    public int PeerReviewDetailId { get; set; }

    public int PeerReviewId { get; set; }

    public int QuestionId { get; set; }

    public int UserAnswerId { get; set; }

    public string? Feedback { get; set; }

    public decimal ScoreEachQuestion { get; set; }

    public virtual PeerReview PeerReview { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;

    public virtual UserAnswer UserAnswer { get; set; } = null!;
}
