using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class StudentOfCourse
{
    public int CouseEnrollmentId { get; set; }

    public int CourseId { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public int? Price { get; set; }

    public bool? Status { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual CoursesEnrollment CouseEnrollment { get; set; } = null!;
}
