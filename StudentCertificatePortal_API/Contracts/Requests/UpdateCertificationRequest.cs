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

        public string? CertValidity { get; set; }
        public int? TypeId { get; set; }

        public int? OrganizeId { get; set; }
        public List<int>? CertIdPrerequisites { get; set; }

        public List<int>? MajorIds { get; set; }
        public List<int>? JobIds { get; set; }
    }
}
