namespace TinyLedger.Models
{
    public class TransactionHistoryResponse
    {
        public List<Transaction> Transactions { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public Guid AccountId { get; set; }
    }
}
