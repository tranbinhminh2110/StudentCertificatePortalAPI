using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class ScoreDto: IMapFrom<Score>
    {
        public int ScoreId { get; set; }

        public int UserId { get; set; }

        public int ExamId { get; set; }

        public decimal ScoreValue { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
