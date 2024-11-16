using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class UserDto: IMapFrom<User>
    {
        public int UserId { get; set; }

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? Email { get; set; }

        public string? Fullname { get; set; }

        public DateTime? Dob { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Role { get; set; }

        public bool? Status { get; set; }

        public DateTime? UserCreatedAt { get; set; }

        public string? UserImage { get; set; }
        public int UserOffenseCount { get; set; }

    }
}
