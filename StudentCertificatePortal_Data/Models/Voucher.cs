﻿using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Voucher
{
    public int VoucherId { get; set; }

    public string? VoucherName { get; set; }

    public string? VoucherDescription { get; set; }

    public int? Percentage { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public bool? VoucherStatus { get; set; }

    public string? VoucherImage { get; set; }

    public string? VoucherLevel { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<SimulationExam> Exams { get; set; } = new List<SimulationExam>();
}
