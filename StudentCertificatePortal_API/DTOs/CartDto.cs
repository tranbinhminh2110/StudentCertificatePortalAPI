﻿using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class CartDto : IMapFrom<Cart>
    {
        public int CartId { get; set; }

        public int? TotalPrice { get; set; }

        public int? UserId { get; set; }
        public List<int>? ExamId { get; set; } = new List<int>();

    }
}