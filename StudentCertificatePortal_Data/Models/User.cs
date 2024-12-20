using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class User
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

    public virtual Cart? Cart { get; set; }

    public virtual ICollection<CoursesEnrollment> CoursesEnrollments { get; set; } = new List<CoursesEnrollment>();

    public virtual ICollection<ExamsEnrollment> ExamsEnrollments { get; set; } = new List<ExamsEnrollment>();

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();

    public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();

    public virtual Wallet? Wallet { get; set; }

    public virtual ICollection<Certification> Certs { get; set; } = new List<Certification>();
}
