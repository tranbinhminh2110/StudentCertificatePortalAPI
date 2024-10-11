namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateCourseEnrollmentRequest
    {
        public DateTime? CourseEnrollmentDate { get; set; }

        public string? CourseEnrollmentStatus { get; set; }

        public int? TotalPrice { get; set; }

        public int? UserId { get; set; }
    }
}
