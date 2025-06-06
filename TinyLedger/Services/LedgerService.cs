using TinyLedger.Models;

namespace TinyLedger.Services
{

    public class LedgerService : ILedgerService
    {
        private readonly Account _account = new Account
        {
            Id = Account.DefaultAccountId,
            IBAN = "ES9121000418450200051332",
            CurrentBalance = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Transactions = new List<Transaction>()
        };

        private static readonly object _lock = new object();

        public Task<AccountResponse> GetAccount()
        {
            lock (_lock)
            {
                var response = new AccountResponse
                {
                    Id = _account.Id,
                    IBAN = _account.IBAN,
                    CurrentBalance = _account.CurrentBalance,
                    CreatedAt = _account.CreatedAt,
                    UpdatedAt = _account.UpdatedAt,
                    TransactionCount = _account.Transactions.Count
                };
                return Task.FromResult(response);
            }
        }

        public Task<Transaction> RecordTransaction(TransactionRequest request)
        {
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Amount must be positive.", nameof(request.Amount));
            }

            lock (_lock)
            {
                if (request.Type == TransactionType.Withdrawal)
                {
                    if (_account.CurrentBalance < request.Amount)
                    {
                        throw new InvalidOperationException("Insufficient balance for withdrawal.");
                    }
                    _account.CurrentBalance -= request.Amount;
                }
                else // Deposit
                {
                    _account.CurrentBalance += request.Amount;
                }

                _account.UpdatedAt = DateTime.UtcNow;

                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    AccountId = Account.DefaultAccountId,
                    Type = request.Type,
                    Amount = request.Amount,
                    Timestamp = DateTime.UtcNow,
                    Description = request.Description ?? string.Empty,
                    BalanceAfterTransaction = _account.CurrentBalance
                };

                _account.Transactions.Add(transaction);

                return Task.FromResult(transaction);
            }
        }

        public Task<BalanceResponse> GetBalance()
        {
            lock (_lock)
            {
                return Task.FromResult(new BalanceResponse
                {
                    Balance = _account.CurrentBalance,
                    AsOf = DateTime.UtcNow,
                    AccountId = _account.Id,
                    AccountIBAN = _account.IBAN
                });
            }
        }

        public Task<TransactionHistoryResponse> GetTransactionHistory(int page = 1, int pageSize = 50)
        {
            lock (_lock)
            {
                var paginatedTransactions = _account.Transactions
                    .OrderByDescending(t => t.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Task.FromResult(new TransactionHistoryResponse
                {
                    Transactions = paginatedTransactions,
                    TotalCount = _account.Transactions.Count,
                    Page = page,
                    PageSize = pageSize,
                    AccountId = _account.Id
                });
            }
        }

        public Task<Transaction?> GetTransaction(Guid transactionId)
        {
            lock (_lock)
            {
                var transaction = _account.Transactions.FirstOrDefault(t => t.Id == transactionId);
                return Task.FromResult(transaction);
            }
        }
    }
}