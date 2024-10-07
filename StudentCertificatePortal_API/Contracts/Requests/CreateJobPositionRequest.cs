namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateJobPositionRequest
    {
        public string? JobPositionCode { get; set; }

        public string? JobPositionName { get; set; }

        public string? JobPositionDescription { get; set; }
    }
}
