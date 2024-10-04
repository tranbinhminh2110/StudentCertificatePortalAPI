using System.ComponentModel.DataAnnotations;

namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class ForgetPasswordRequest
    {
        [Required]
        public string? Email { get; set; }
    }
}
