namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateCartRequest
    {
        public int? UserId { get; set; }
        public List<int>? ExamId { get; set; } = new List<int>();
        public List<int>? CourseId { get; set; } = new List<int>();

    }
}
