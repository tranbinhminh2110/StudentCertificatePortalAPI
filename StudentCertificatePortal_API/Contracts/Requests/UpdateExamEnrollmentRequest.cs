namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateExamEnrollmentRequest
    {

        public DateTime? ExamEnrollmentDate { get; set; }

        public string? ExamEnrollmentStatus { get; set; }

        public int? TotalPrice { get; set; }

        public int? UserId { get; set; }
    }
}
