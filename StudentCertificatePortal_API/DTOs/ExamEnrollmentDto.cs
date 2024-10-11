namespace StudentCertificatePortal_API.DTOs
{
    public class ExamEnrollmentDto
    {
        public int ExamEnrollmentId { get; set; }

        public DateTime? ExamEnrollmentDate { get; set; }

        public string? ExamEnrollmentStatus { get; set; }

        public int? TotalPrice { get; set; }

        public int? UserId { get; set; }
    }
}
