namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class UpdateTransactionRequest
    {
        public int WalletId { get; set; }
        public int Point { get; set; }

        public string? TransDesription { get; set; }
        public string TransStatus { get; set; } = null!;
    }
}
