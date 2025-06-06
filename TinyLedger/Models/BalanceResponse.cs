namespace TinyLedger.Models
{
    public class BalanceResponse
    {
        public decimal Balance { get; set; }
        public DateTime AsOf { get; set; }
        public Guid AccountId { get; set; }
        public string AccountIBAN { get; set; } = string.Empty;
    }
}
