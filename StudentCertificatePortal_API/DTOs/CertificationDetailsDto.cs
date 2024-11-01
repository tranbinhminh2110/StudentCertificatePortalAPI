namespace StudentCertificatePortal_API.DTOs
{
    public class CertificationDetailsDto
    {
        public int CertId { get; set; }
        public string? CertName { get; set; }

        public string? CertCode { get; set; }

        public string? CertDescription { get; set; }
        public string? CertImage { get; set; }
        public string? TypeName { get; set; }
        public string? OrganizeName { get; set; }

        public string? CertValidity { get; set; }
        public string? Permission { get; set; }
    }
}
