namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreatePaymentRequest
    {
        public int? UserId { get; set; }

        public int? ExamEnrollmentId { get; set; }

        public int? CourseEnrollmentId { get; set; }
    }
}
