using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class PeerReviewDto: IMapFrom<PeerReview>
    {
        public int PeerReviewId { get; set; }

        public int ReviewerId { get; set; }

        public int ReviewedUserId { get; set; }

        public int ScoreId { get; set; }

        public decimal ScorePeerReviewer { get; set; }

        public string FeedbackPeerReviewer { get; set; } = null!;

        public DateTime ReviewDate { get; set; }

        public List<UserAnswerForEssayDto> UserAnswers { get; set; } = new List<UserAnswerForEssayDto>();
    }
    public class UserAnswerForEssayDto
    {
        public int UserAnswerId { get; set; }
        public int QuestionId { get; set; }
        public decimal ScoreValue { get; set; }
        public string Answer { get; set; }
    }

}
