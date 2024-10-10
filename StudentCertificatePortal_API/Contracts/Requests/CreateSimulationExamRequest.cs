namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateSimulationExamRequest
    {
        public string? ExamName { get; set; }

        public string? ExamCode { get; set; }

        public int? CertId { get; set; }

        public string? ExamDescription { get; set; }

        public int? ExamFee { get; set; }

        public int? ExamDiscountFee { get; set; }

        public string? ExamImage { get; set; }
    }
}
