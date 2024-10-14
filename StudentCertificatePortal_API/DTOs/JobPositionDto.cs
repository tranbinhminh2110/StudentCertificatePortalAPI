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

        public List<int>? MajorId { get; set; } = new List<int>();

        public List<string>? MajorName { get; set; } = new List<string>();
        public List<string>? MajorCode { get; set; } = new List<string>();
        public List<string>? MajorDescription { get; set; } = new List<string>();
        public List<int>? CertId { get; set; } = new List<int>();
        public List<string>? CertName { get; set; } = new List<string>();
        public List<string>? CertCode { get; set; } = new List<string>();
        public List<string>? CertDescription { get; set; } = new List<string>();



    }
}
