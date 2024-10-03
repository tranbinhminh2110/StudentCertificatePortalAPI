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

    }
}
