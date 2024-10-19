namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateCourseRequest
    {
        public string? CourseName { get; set; }

        public string? CourseCode { get; set; }

        public string? CourseTime { get; set; }

        public string? CourseDescription { get; set; }
        public int? CourseFee { get; set; }
        public List<int> VoucherIds { get; set; } = new List<int>();

        public string? CourseImage { get; set; }

        public int? CertId { get; set; }
    }
}
