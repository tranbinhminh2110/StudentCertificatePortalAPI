namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateExamSessionRequest
    {
        public string? SessionName { get; set; }

        public string? SessionCode { get; set; }

        public DateTime? SessionDate { get; set; }

        public string? SessionAddress { get; set; }

        public int? CertId { get; set; }

        public DateTime? SessionCreatedAt { get; set; }
    }
}
