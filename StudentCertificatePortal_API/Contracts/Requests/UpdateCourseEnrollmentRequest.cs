namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateCourseEnrollmentRequest
    {
        public int? UserId { get; set; }

        public List<int> Courses { get; set; } = new List<int>();
    }
}
