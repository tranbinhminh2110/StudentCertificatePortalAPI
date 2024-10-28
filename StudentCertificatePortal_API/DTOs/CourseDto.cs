using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class CourseDto: IMapFrom<Course>
    {
        public int CourseId { get; set; }

        public string? CourseName { get; set; }

        public string? CourseCode { get; set; }

        public string? CourseTime { get; set; }

        public string? CourseDescription { get; set; }
        public int? CourseFee { get; set; }
        public int? CourseDiscountFee { get; set; }

        public string? CourseImage { get; set; }

        public int? CertId { get; set; }
        public string? CoursePermission { get; set; }

        public List<CertificationDetailsDto> CertificationDetails { get; set; } = new List<CertificationDetailsDto>();        
        public List<VoucherDetailsDto> VoucherDetails { get; set; } = new List<VoucherDetailsDto>();

    }
}
