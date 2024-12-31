namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdatePeerReviewRequest
    {
        public int ReviewerId { get; set; }

        public decimal ScorePeerReviewer { get; set; }

        public string FeedbackPeerReviewer { get; set; } = null!;

        public List<PeerReviewQuestionScore> peerReviewQuestionScores { get; set; } = new List<PeerReviewQuestionScore>();
    }

    public class PeerReviewQuestionScore
    {
        public int QuestionId { get; set; }
        public string FeedBackForQuestion { get; set; }

        public decimal ScoreForQuestion { get; set; }
    }

}
