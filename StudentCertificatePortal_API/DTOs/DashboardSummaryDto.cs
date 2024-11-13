namespace StudentCertificatePortal_API.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalCertificates { get; set; }
        public int TotalMajor{ get; set; }

        public int TotalCourses { get; set; }
        public int TotalJobsPosition { get; set; }
        public int TotalSimulationExams { get; set; }
        public int TotalStudents { get; set; }
        public int TotalPoint {  get; set; }
    }
}
