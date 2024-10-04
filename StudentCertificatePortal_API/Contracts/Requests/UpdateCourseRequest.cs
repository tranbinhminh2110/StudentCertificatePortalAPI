namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateCourseRequest
    {
        public string? CourseName { get; set; }

        public string? CourseCode { get; set; }

        public string? CourseTime { get; set; }

        public string? CourseDescription { get; set; }

        public int? CertId { get; set; }
    }
}
