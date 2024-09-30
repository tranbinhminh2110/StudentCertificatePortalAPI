namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateUserRequest
    {
        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? Email { get; set; }

        public string? Fullname { get; set; }

        public DateTime? Dob { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Role { get; set; }

        public bool? Status { get; set; }
        public string? UserImage { get; set; }

    }
}
