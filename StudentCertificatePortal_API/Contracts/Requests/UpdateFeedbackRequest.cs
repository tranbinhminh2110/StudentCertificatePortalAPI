namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateFeedbackRequest
    {
        public int? FeedbackRatingvalue { get; set; }
        public string? FeedbackDescription { get; set; }
        public string? FeedbackImage { get; set; }
    }
}
