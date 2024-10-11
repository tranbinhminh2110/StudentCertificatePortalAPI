namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateExamEnrollmentRequest
    {
        public DateTime? ExamEnrollmentDate { get; set; }

        public string? ExamEnrollmentStatus { get; set; }

        public int? TotalPrice { get; set; }

        public int? UserId { get; set; }
    }
}
