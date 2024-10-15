namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreatePaymentRequest
    {

        public DateTime? PaymentDate { get; set; }

        public int? PaymentAmount { get; set; }

        public string? PaymentMethod { get; set; }

        public string? PaymentStatus { get; set; }

        public int? WalletId { get; set; }

        public int? ExamEnrollmentId { get; set; }

        public int? CourseEnrollmentId { get; set; }
    }
}
