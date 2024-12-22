namespace StudentCertificatePortal_API.DTOs
{
    public class QuestionReviewDto
    {
        public int QuestionId { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public List<int>? UserAnswersForChoice { get; set; }
        public string? UserAnswerContentForEssay { get; set; } 
        public List<AnswerDto> SystemAnswers { get; set; } = new List<AnswerDto>();
        public bool IsCorrectQuestion { get; set; }
        public decimal ScoreValue { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

}
