using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class CoursesEnrollment
{
    public int CourseEnrollmentId { get; set; }

    public DateTime? CourseEnrollmentDate { get; set; }

    public string? CourseEnrollmentStatus { get; set; }

    public int? TotalPrice { get; set; }

    public int? UserId { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<StudentOfCourse> StudentOfCourses { get; set; } = new List<StudentOfCourse>();

    public virtual User? User { get; set; }
}
