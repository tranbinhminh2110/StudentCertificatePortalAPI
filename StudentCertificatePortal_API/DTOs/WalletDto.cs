using StudentCertificatePortal_API.Mapping;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.DTOs
{
    public class WalletDto: IMapFrom<Wallet>
    {
        public int WalletId { get; set; }

        public int? Point { get; set; }

        public int? UserId { get; set; }

        public DateTime? DepositDate { get; set; }

        public string? History { get; set; }

        public string? WalletStatus { get; set; }
    }
}
