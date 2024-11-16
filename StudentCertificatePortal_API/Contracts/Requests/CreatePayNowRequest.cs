namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreatePayNowRequest
    {
        public int? UserId { get; set; }

        public List<int> Simulation_Exams { get; set; } = new List<int>();

        public List<int> Courses { get; set; } = new List<int>();
    }
}
