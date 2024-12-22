namespace StudentCertificatePortal_API.DTOs
{
    public class ExamReviewDto
    {
        public int ExamId { get; set; }
        public int UserId { get; set; }
        public decimal TotalScore { get; set; }
        public List<QuestionReviewDto> Questions { get; set; } = new List<QuestionReviewDto>();
    }
}
