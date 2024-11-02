namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateMajorRequest
    {
        public string? MajorCode { get; set; }

        public string? MajorName { get; set; }

        public string? MajorDescription { get; set; }
        public string? MajorImage { get; set; }

        public List<int>? JobPositionId { get; set; } = new List<int>();
        public List<int>? CertId { get; set; } = new List<int>();


    }
}
