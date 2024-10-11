using AutoMapper.Configuration.Annotations;
using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateQuestionRequest
    {
        public string? QuestionName { get; set; }
        public int ExamId { get; set; }

        public List<AnswerRequest>? Answers { get; set; }
    }

    public class AnswerRequest
    {
        public string? Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}
