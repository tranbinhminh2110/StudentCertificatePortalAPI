using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class VoucherDto : IMapFrom<Voucher>
    {
        public int VoucherId { get; set; }

        public string? VoucherName { get; set; }

        public string? VoucherDescription { get; set; }

        public int? Percentage { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool? VoucherStatus { get; set; }
        public string? VoucherImage { get; set; }

        public string? VoucherLevel { get; set; }
        public List<int>? ExamId { get; set; } = new List<int>();
        public List<int>? CourseId { get; set; } = new List<int>();
        public List<ExamDetailsDto> ExamDetails { get; set; } = new List<ExamDetailsDto>();
        public List<CourseDetailsDto> CourseDetails { get; set; } = new List<CourseDetailsDto>();

    }
}
