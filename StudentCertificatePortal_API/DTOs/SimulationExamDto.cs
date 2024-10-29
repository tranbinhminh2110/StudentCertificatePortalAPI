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
        public string? ExamPermission { get; set; }

        public List<ExamList> ListQuestions { get; set; } = new List<ExamList>();
        public List<CertificationDetailsDto> CertificationDetails { get; set; } = new List<CertificationDetailsDto>();
        public List<VoucherDetailsDto> VoucherDetails { get; set; } = new List<VoucherDetailsDto>();
    }
    public class ExamList
    {
        public int QuestionId { get; set; }

        public string? QuestionName { get; set; }    
        public List<AnswerList> Answers { get; set; } = new List<AnswerList>();
    }
    public class AnswerList
    {
        public int AnswerId { get; set; }
        public string? AnswerText { get;set; }
    }
}
