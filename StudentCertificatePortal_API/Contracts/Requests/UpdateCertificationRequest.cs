namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateCertificationRequest
    {

        public string? CertName { get; set; }

        public string? CertCode { get; set; }

        public string? CertDescription { get; set; }

        public int? CertCost { get; set; }

        public string? CertPointSystem { get; set; }

        public string? CertImage { get; set; }

        public string? CertPrerequisite { get; set; }

        public DateTime? ExpiryDate { get; set; }
    }
}
