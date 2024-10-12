using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class CertTypeDto: IMapFrom<CertType>
    {
        public int TypeId { get; set; }

        public string? TypeCode { get; set; }

        public string? TypeName { get; set; }
    }
}
