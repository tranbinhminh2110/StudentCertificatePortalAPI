namespace StudentCertificatePortal_API.DTOs
{
    public class VoucherDetailsDto
    {
        public int VoucherId { get; set; }

        public string? VoucherName { get; set; }

        public string? VoucherDescription { get; set; }

        public int? Percentage { get; set; }

        public DateTime? CreationDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public bool? VoucherStatus { get; set; }
    }
}
