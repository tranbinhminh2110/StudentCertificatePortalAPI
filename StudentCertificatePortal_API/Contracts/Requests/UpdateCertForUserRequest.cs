using System.ComponentModel.DataAnnotations;

namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateCertForUserRequest
    {
        [Required]
        public int UserId { get; set; }

        public List<int> CertificationId { get; set; } = new List<int>();
    }
}
