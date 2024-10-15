using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class PaymentDto: IMapFrom<Payment>
    {
        public int PaymentId { get; set; }

        public DateTime? PaymentDate { get; set; }

        public int? PaymentAmount { get; set; }

        public string? PaymentMethod { get; set; }

        public string? PaymentStatus { get; set; }

        public int? WalletId { get; set; }

        public int? ExamEnrollmentId { get; set; }

        public int? CourseEnrollmentId { get; set; }
    }
}
