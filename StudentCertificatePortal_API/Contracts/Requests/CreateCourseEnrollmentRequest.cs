namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateCourseEnrollmentRequest
    {
        public int? UserId { get; set; }

        public List<int> Courses { get; set; } = new List<int>();
    }
}
