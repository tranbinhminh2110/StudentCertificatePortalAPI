namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string email, string subject, string message);
    }
}
