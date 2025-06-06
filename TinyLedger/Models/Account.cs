namespace TinyLedger.Models
{
    public class Account
    {
        // Fixed account ID for single account approach
        public static readonly Guid DefaultAccountId = new Guid("12345678-1234-5678-9abc-123456789012");

        public Guid Id { get; set; } = DefaultAccountId;
        public string IBAN { get; set; } = "ES9121000418450200051332"; // Default Spanish IBAN
        public decimal CurrentBalance { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
