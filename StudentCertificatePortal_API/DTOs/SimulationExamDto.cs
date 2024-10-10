using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class SimulationExamDto: IMapFrom<SimulationExam>
    {
        public int ExamId { get; set; }

        public string? ExamName { get; set; }

        public string? ExamCode { get; set; }

        public int? CertId { get; set; }

        public string? ExamDescription { get; set; }

        public int? ExamFee { get; set; }

        public int? ExamDiscountFee { get; set; }

        public string? ExamImage { get; set; }
    }
}
