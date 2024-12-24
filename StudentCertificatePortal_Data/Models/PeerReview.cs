using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class PeerReview
{
    public int PeerReviewId { get; set; }

    public int ReviewerId { get; set; }

    public int? ReviewedUserId { get; set; }

    public int ScoreId { get; set; }

    public decimal? ScorePeerReviewer { get; set; }

    public string? FeedbackPeerReviewer { get; set; }

    public DateTime ReviewDate { get; set; }

    public virtual User? ReviewedUser { get; set; }

    public virtual User Reviewer { get; set; } = null!;

    public virtual Score Score { get; set; } = null!;
}
