using TinyLedger.Models;

namespace TinyLedger.Services
{
    // Service Interface
    public interface ILedgerService
    { 
        // Account operations (single account)
        Task<AccountResponse> GetAccount();

        // Transaction operations
        Task<Transaction> RecordTransaction(TransactionRequest request);
        Task<BalanceResponse> GetBalance();
        Task<TransactionHistoryResponse> GetTransactionHistory(int page = 1, int pageSize = 50);
        Task<Transaction?> GetTransaction(Guid transactionId);
    }
}