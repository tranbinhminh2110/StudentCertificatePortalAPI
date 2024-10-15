namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreatePaymentRequest
    {
        public int? WalletId { get; set; }

        public int? ExamEnrollmentId { get; set; }

        public int? CourseEnrollmentId { get; set; }
    }
}
