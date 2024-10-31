using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Contracts.Responses
{
    public class PaymentRequestResponse
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public PaymentData Data { get; set; }
        public string Signature { get; set; }
    }


    public class PaymentData
    {
        public string Id { get; set; }
        public int OrderCode { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal AmountRemaining { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<TransactionPayOS> Transactions { get; set; }
        public string CancellationReason { get; set; }
        public DateTime? CanceledAt { get; set; }
    }

    public class TransactionPayOS
    {
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public string AccountNumber { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public string VirtualAccountName { get; set; }
        public string VirtualAccountNumber { get; set; }
        public string CounterAccountBankId { get; set; }
        public string CounterAccountBankName { get; set; }
        public string CounterAccountName { get; set; }
        public string CounterAccountNumber { get; set; }
    }
}
