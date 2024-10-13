using System;
using System.Collections.Generic;

namespace StudentCertificatePortal_Data.Models;

public partial class Transaction
{
    public int TransactionId { get; set; }

    public int WalletId { get; set; }

    public string? TransDesription { get; set; }

    public int Point { get; set; }

    public int Amount { get; set; }

    public DateTime CreatedAt { get; set; }

    public string TransStatus { get; set; } = null!;

    public virtual Wallet Wallet { get; set; } = null!;
}
