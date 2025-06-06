namespace TinyLedger.Models
{
    public class AccountResponse
    {
        public Guid Id { get; set; }
        public string IBAN { get; set; } = string.Empty;
        public decimal CurrentBalance { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int TransactionCount { get; set; }
    }
}
