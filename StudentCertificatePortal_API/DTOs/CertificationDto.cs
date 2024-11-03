using System;
using System.Collections.Generic;
using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class CertificationDto : IMapFrom<Certification>
    {
        public int CertId { get; set; }

        public string? CertName { get; set; }

        public string? CertCode { get; set; }

        public string? CertDescription { get; set; }

        public int? CertCost { get; set; }

        public string? CertPointSystem { get; set; }

        public string? CertImage { get; set; }

        public string? CertValidity { get; set; }
        public int? OrganizeId { get; set; }
        public string? OrganizeName { get; set; }
        public int? TypeId { get; set; }
        public string? TypeName { get; set; }
        public string? Permission { get; set; }
        
        


        // Danh sách ID của các chứng chỉ tiền đề
        public List<int>? CertPrerequisiteId { get; set; } = new List<int>();
        public List<string>? CertPrerequisite { get; set; } = new List<string>();
        public List<string>? CertCodePrerequisite { get; set; } = new List<string>();
        public List<string>? CertDescriptionPrerequisite { get; set; } = new List<string>();
        

        // Danh sách ID của JobPositions 
        public List<int>? JobPositionIds { get; set; } = new List<int>();
        public List<string>? JobPositionCodes { get; set; } = new List<string>();
        public List<string>? JobPositionNames { get; set; } = new List<string>();
        public List<string>? JobPositionDescriptions { get; set; } = new List<string>();
        public List<string>? JobPositionPermission { get; set; } = new List<string>();

        // Danh sách ID của Majors
        public List<int>? MajorIds { get; set; } = new List<int>();
        public List<string>? MajorCodes { get; set; } = new List<string>();
        public List<string>? MajorNames { get; set; } = new List<string>();
        public List<string>? MajorDescriptions { get; set; } = new List<string>();
        public List<string>? MajorPermission { get; set; } = new List<string>();

    }
}
