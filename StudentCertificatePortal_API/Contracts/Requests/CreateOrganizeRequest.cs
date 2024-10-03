namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateOrganizeRequest
    {
        public string? OrganizeName { get; set; }

        public string? OrganizeAddress { get; set; }

        public string? OrganizeContact { get; set; }
    }
}
