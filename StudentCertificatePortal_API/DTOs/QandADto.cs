using AutoMapper.Configuration.Annotations;
using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class QandADto : IMapFrom<Question>
    {
        public int QuestionId { get; set; }
        public string? QuestionName { get; set; }
        public int? ExamId { get; set; }

        [Ignore]
        public List<AnswerDto>? Answers { get; set; }
    }

    public class AnswerDto : IMapFrom<Answer>
    {
        public int AnswerId { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
    }
}
