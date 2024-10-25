using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class ExamEnrollmentDto: IMapFrom<ExamsEnrollment>
    {
        public int ExamEnrollmentId { get; set; }

        public DateTime? ExamEnrollmentDate { get; set; }

        public string? ExamEnrollmentStatus { get; set; }

        public int? TotalPrice { get; set; }

        public int? UserId { get; set; }

        public List<ExamDetailsDto> SimulationExamDetail { get; set; } = new List<ExamDetailsDto>();
    }
}
