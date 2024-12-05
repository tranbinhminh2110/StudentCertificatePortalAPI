using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class FeedbackDto : IMapFrom<Feedback>
    {
        public int FeedbackId { get; set; }
        public int? FeedbackRatingvalue { get; set; }

        public string? FeedbackDescription { get; set; }

        public int? UserId { get; set; }

        public int? ExamId { get; set; }

        public DateTime? FeedbackCreatedAt { get; set; }

        public string? FeedbackImage { get; set; }
        public bool? FeedbackPermission { get; set; }

        public string? ExamPermission { get; set; }
        public UserDetailsDto? UserDetails { get; set; }

    }
}
