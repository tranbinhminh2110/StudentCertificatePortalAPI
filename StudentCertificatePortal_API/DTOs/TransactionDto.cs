using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class TransactionDto: IMapFrom<Transaction>
    {
        public int TransactionId { get; set; }

        public int WalletId { get; set; }

        public string? TransDesription { get; set; }

        public int Point { get; set; }

        public int Amount { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
