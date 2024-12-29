namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateExamEnrollmentVoucherRequest
    {
        public int? UserId { get; set; }

        public List<int> Simulation_Exams { get; set; } = new List<int>();
        public int? VoucherId { get; set; } 

    }
}
