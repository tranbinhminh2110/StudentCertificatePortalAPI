using System.ComponentModel.DataAnnotations;

namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class CreateTransactionRequest
    {
        public int WalletId { get; set; }
        public int Point { get; set; }
    }
}
