namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateJobPositionRequest
    {
        public string? JobPositionCode { get; set; }

        public string? JobPositionName { get; set; }

        public string? JobPositionDescription { get; set; }
        public List<int>? MajorId { get; set; } = new List<int>();

/*        public List<int>? CertId { get; set; } = new List<int>();
*/    }
}
