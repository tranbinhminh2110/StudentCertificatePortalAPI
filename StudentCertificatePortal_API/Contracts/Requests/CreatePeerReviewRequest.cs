namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreatePeerReviewRequest
    {
        public int ReviewerId { get; set; }
        public int ScoreId { get; set; }
    }
}
