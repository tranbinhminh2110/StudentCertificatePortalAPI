﻿using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class FeedbackDto : IMapFrom<Feedback>
    {
        public int FeedbackId { get; set; }

        public string? FeedbackDescription { get; set; }

        public int? UserId { get; set; }

        public int? ExamId { get; set; }

        public DateTime? FeedbackCreatedAt { get; set; }

        public string? FeedbackImage { get; set; }
    }
}
