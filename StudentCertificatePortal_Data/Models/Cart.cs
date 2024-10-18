using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Cart
{
    public int CartId { get; set; }

    public int? TotalPrice { get; set; }

    public int? UserId { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<SimulationExam> Exams { get; set; } = new List<SimulationExam>();
}
