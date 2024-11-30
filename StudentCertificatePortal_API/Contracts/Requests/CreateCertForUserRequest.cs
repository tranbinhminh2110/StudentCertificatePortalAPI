using System.ComponentModel.DataAnnotations;

namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateCertForUserRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number")]
        public int UserId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "At least one certification must be selected")]
        public List<int> CertificationId { get; set; } = new List<int>();
    }
}
