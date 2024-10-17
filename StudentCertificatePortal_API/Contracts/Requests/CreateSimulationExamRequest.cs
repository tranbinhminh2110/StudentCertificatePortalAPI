namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateSimulationExamRequest
    {
        public string? ExamName { get; set; }

        public string? ExamCode { get; set; }

        public int? CertId { get; set; }

        public string? ExamDescription { get; set; }

        public int? ExamFee { get; set; }

        public List<int> VoucherIds { get; set; } = new List<int>();    

        public string? ExamImage { get; set; }


    }
}
