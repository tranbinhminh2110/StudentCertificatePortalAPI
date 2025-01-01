using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class PeerReviewForReviewerDto : IMapFrom<PeerReview>
    {
        public int PeerReviewId { get; set; }
        public int ReviewedUserId { get; set; }
        public string? ReviewedUserName { get; set; }
        public string? ExamName { get; set; }
        public int ScoreId { get; set; }
        public double MaxQuestionScore { get; set; }
    }
}
