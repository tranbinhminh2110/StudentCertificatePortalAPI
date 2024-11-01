using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class ExamSessionDto : IMapFrom<ExamSession>
    {
        public int SessionId { get; set; }

        public string? SessionName { get; set; }

        public string? SessionCode { get; set; }

        public DateTime? SessionDate { get; set; }

        public string? SessionAddress { get; set; }

        public int? CertId { get; set; }

        public DateTime? SessionCreatedAt { get; set; }
        public string? SessionTime { get; set; }

    }
}
