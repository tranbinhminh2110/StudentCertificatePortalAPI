namespace StudentCertificatePortal_API.DTOs
{
    public class PeerReviewForReviewedUserDto
    {
        public int PeerReviewId { get; set; }
        public int ReviewerId { get; set; }
        public int ReviewedUserId { get; set; }
        public string? ReviewerName { get; set; }
        public int ScoreId { get; set; }
        public string? ExamName { get; set; } 
        public decimal ScorePeerReviewer { get; set; }
        public string FeedbackPeerReviewer { get; set; } = null!;
        public DateTime ReviewDate { get; set; }
        public double MaxQuestionScore { get; set; }
       
    }
}
