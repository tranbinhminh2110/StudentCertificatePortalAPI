﻿namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateVoucherRequest
    {
        public string? VoucherName { get; set; }

        public string? VoucherDescription { get; set; }

        public int? Percentage { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool? VoucherStatus { get; set; }
        public List<int>? ExamId { get; set; } = new List<int>();
    }
}