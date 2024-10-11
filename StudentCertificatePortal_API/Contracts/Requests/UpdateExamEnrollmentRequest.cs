namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateExamEnrollmentRequest
    {

        public int? UserId { get; set; }

        public List<int> Simulation_Exams { get; set; } = new List<int>();
    }
}
