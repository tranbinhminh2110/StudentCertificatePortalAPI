namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateQuestionRequest {
        public string? QuestionName { get; set; }
        public int ExamId { get; set; }

        public List<AnswerRequest>? Answers { get; set; }
    }
}
