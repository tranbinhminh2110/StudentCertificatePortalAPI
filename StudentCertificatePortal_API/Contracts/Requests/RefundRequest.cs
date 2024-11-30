using System.ComponentModel.DataAnnotations;

namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class RefundRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "WalletId must be greater than 0.")]
        public int WalletId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Point must be greater than 0.")]
        public int Point { get; set; }
    }
}
