namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateCourseEnrollmentRequest
    {
        public List<int> Courses { get; set; } = new List<int>();
    }
}
