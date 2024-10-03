using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class OrganizeDto : IMapFrom<Organize>
    {
        public int OrganizeId { get; set; }

        public string? OrganizeName { get; set; }

        public string? OrganizeAddress { get; set; }

        public string? OrganizeContact { get; set; }

    }
}
