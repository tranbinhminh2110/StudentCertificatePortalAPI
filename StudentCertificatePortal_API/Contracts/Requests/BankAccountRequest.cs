using System.ComponentModel.DataAnnotations;

namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class BankAccountRequest
    {
        [Required(ErrorMessage = "Account number is required.")]
        [StringLength(20, ErrorMessage = "Account number cannot exceed 20 characters.")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "Bank name is required.")]
        [StringLength(100, ErrorMessage = "Bank name cannot exceed 100 characters.")]
        public string BankName { get; set; }
    }
}
