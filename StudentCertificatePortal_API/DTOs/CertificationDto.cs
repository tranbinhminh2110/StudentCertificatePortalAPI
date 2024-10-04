using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class CertificationDto: IMapFrom<Certification>
    {
        public int CertId { get; set; }

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
