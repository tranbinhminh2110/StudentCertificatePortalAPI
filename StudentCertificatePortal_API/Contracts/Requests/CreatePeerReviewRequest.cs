namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreatePeerReviewRequest
    {
        public int ReviewedUserId { get; set; }
        public int ScoreId { get; set; }
    }
}
