using AutoMapper;
using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class MajorDto : IMapFrom<Major>
    {
        public int MajorId { get; set; }

        public string? MajorCode { get; set; }

        public string? MajorName { get; set; }

        public string? MajorDescription { get; set; }
        public string? MajorImage { get; set; }

        public string? MajorPermission { get; set; }

        public List<int>? JobPositionId { get; set; } = new List<int>();
        public List<int>? CertId { get; set; } = new List<int>();

        public List<JobPositionDetailsDto> JobPositionDetails { get; set; } = new List<JobPositionDetailsDto>();
        public List<CertificationDetailsDto> CertificationDetails { get; set; } = new List<CertificationDetailsDto>();



    }
}
