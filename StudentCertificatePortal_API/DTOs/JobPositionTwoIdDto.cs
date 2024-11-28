using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class JobPositionTwoIdDto : IMapFrom<JobPosition>
    {
        public int JobPositionId { get; set; }

        public string? JobPositionCode { get; set; }

        public string? JobPositionName { get; set; }

        public string? JobPositionDescription { get; set; }
        public string? JobPositionPermission { get; set; }
        public int? OrganizeId { get; set; }
        public List<CertificationTwoIdDto> CertificationTwoId { get; set; } = new List<CertificationTwoIdDto>();
    }
}
