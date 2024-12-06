using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class JobPositionDto : IMapFrom<JobPosition>
    {
        public int JobPositionId { get; set; }

        public string? JobPositionCode { get; set; }

        public string? JobPositionName { get; set; }

        public string? JobPositionDescription { get; set; }
        public string? JobPositionPermission { get; set; }


        public List<int>? MajorId { get; set; } = new List<int>();
        public List<int>? CertId { get; set; } = new List<int>();

        public List<MajorDetailsDto> MajorDetails { get; set; } = new List<MajorDetailsDto>();
        public List<CertOrganizeDto> CertOrganizes { get; set; } = new List<CertOrganizeDto>();
        public List<CertificationDetailsDto> CertificationDetails { get; set; } = new List<CertificationDetailsDto>();
        public List<CertificationTwoIdDto> CertificationTwoId { get; set; } = new List<CertificationTwoIdDto>();

    }
}
