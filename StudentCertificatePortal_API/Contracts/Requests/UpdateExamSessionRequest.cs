namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateExamSessionRequest
    {
        public string? SessionName { get; set; }

        public string? SessionCode { get; set; }

        public DateTime? SessionDate { get; set; }

        public string? SessionAddress { get; set; }
    }
}
