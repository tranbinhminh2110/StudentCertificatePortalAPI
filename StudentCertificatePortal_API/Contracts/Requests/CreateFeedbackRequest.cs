namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateFeedbackRequest
    {
        public int? UserId { get; set; }

        public int? ExamId { get; set; }
        public int? FeedbackRatingvalue { get; set; }

        public string? FeedbackDescription { get; set; }

        public string? FeedbackImage { get; set; }

        public DateTime? FeedbackCreatedAt { get; set; }

    }
}
