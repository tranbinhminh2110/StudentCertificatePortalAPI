using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Wallet
{
    public int WalletId { get; set; }

    public int? Point { get; set; }

    public int? UserId { get; set; }

    public DateTime? DepositDate { get; set; }

    public string? History { get; set; }

    public string? WalletStatus { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User? User { get; set; }
}
