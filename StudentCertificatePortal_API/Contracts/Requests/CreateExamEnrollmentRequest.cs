namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateExamEnrollmentRequest
    {

        public int? UserId { get; set; }

        public List<int> Simulation_Exams { get; set; } = new List<int>();

    }
}
