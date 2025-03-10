﻿using StudentCertificatePortal_API.Enums;

namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UserAnswerRequest
    {
        public int UserId { get; set; }
        public int ExamId { get; set; }
        public List<QuestionRequest> QuestionRequests { get; set; } = new List<QuestionRequest>();
    }

    public class QuestionRequest
    {
        public int QuestionId { get; set;}
        public EnumQuestionType QuestionType { get; set; }
        public List<int> UserAnswerId { get; set; } = new List<int>();
        public string UserAnswerText { get; set; } = string.Empty;
    }

}
