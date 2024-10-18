using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string? CourseName { get; set; }

    public string? CourseCode { get; set; }

    public string? CourseTime { get; set; }

    public string? CourseDescription { get; set; }

    public int? CertId { get; set; }

    public int? CourseFee { get; set; }

    public int? CourseDiscountFee { get; set; }

    public string? CourseImage { get; set; }

    public virtual Certification? Cert { get; set; }

    public virtual ICollection<StudentOfCourse> StudentOfCourses { get; set; } = new List<StudentOfCourse>();

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Voucher> Vouchers { get; set; } = new List<Voucher>();
}
