namespace StudentCertificatePortal_API.DTOs
{
    public class CheckoutDto
    {
        public int OrderCode { get; set; }
        public int Amount { get; set; }
        public string? Description { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerEmail { get; set; }

        public string? BuyerPhone { get; set; }
        public string? BuyerAddress { get; set; }
        public List<WalletRequest> Items { get; set; } = new List<WalletRequest>();
        public string? CancelUrl { get; set; }

        public string? ReturnUrl { get; set; }
        public int ExpiredAt { get; set; }

        public string? Signature { get; set; }
    }

    public class WalletRequest
    {
        public string name { get; set; } 
        public int quantity { get; set; }
        public float price { get; set; }
    }
}
