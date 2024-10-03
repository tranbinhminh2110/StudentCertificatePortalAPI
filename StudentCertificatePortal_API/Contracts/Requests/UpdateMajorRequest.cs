namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateMajorRequest
    {
        public string? MajorCode { get; set; }

        public string? MajorName { get; set; }

        public string? MajorDescription { get; set; }
    }
}
