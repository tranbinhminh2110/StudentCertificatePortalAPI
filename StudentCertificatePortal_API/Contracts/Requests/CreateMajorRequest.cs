﻿using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateMajorRequest
    {
        public string? MajorCode { get; set; }

        public string? MajorName { get; set; }

        public string? MajorDescription { get; set; }
        public List<int>? JobPositionId { get; set; } 

    }
}
