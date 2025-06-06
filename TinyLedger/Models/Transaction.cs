using System.ComponentModel.DataAnnotations;

namespace TinyLedger.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public Guid AccountId { get; set; } = Account.DefaultAccountId; // Always uses the default account
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Description { get; set; }
        public decimal BalanceAfterTransaction { get; set; } // Account balance after this transaction
    }
}