﻿namespace StudentCertificatePortal_API.DTOs
{
    public class ListStudentDto
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? UserImage { get; set; }
        public string? Email { get; set; }
        public string? Fullname { get; set; }
        public DateTime? Dob { get; set; }

        public string? Address { get; set; }

        public string? PhoneNumber { get; set; }

    }
}
