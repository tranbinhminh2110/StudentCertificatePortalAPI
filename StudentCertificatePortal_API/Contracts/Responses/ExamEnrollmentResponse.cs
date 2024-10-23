using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Contracts.Responses
{
    public class ExamEnrollmentResponse
    {
        public bool IsEnrolled { get; set; }
        public string Status { get; set; }
        public int? ExamEnrollmentId { get; set; }
        public string Message { get; set; }

        public ExamEnrollmentDto ExamEnrollment { get; set; }
    }
}
