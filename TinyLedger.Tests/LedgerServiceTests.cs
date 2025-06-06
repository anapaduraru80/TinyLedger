using TinyLedger.Models;
using TinyLedger.Services;

namespace TinyLedger.Tests.Services
{
    public class LedgerServiceTests
    {
        private readonly LedgerService _ledgerService;

        public LedgerServiceTests()
        {
            _ledgerService = new LedgerService();
        }

        [Fact]
        public async Task RecordTransaction_Deposit_IncreasesBalance()
        {
            // Arrange
            var request = new TransactionRequest
            {
                Type = TransactionType.Deposit,
                Amount = 100.50m,
                Description = "Test deposit"
            };

            // Act
            var transaction = await _ledgerService.RecordTransaction(request);
            var balance = await _ledgerService.GetBalance();

            // Assert
            Assert.Equal(100.50m, transaction.BalanceAfterTransaction);
            Assert.Equal(100.50m, balance.Balance);
        }

        [Fact]
        public async Task RecordTransaction_Withdrawal_DecreasesBalance()
        {
            // Arrange - First deposit money
            await _ledgerService.RecordTransaction(new TransactionRequest
            {
                Type = TransactionType.Deposit,
                Amount = 200m,
                Description = "Initial deposit"
            });

            // Act
            var withdrawal = await _ledgerService.RecordTransaction(new TransactionRequest
            {
                Type = TransactionType.Withdrawal,
                Amount = 75m,
                Description = "Test withdrawal"
            });

            // Assert
            Assert.Equal(125m, withdrawal.BalanceAfterTransaction);
        }

        [Fact]
        public async Task RecordTransaction_InsufficientFunds_ThrowsException()
        {
            // Arrange
            var request = new TransactionRequest
            {
                Type = TransactionType.Withdrawal,
                Amount = 100m,
                Description = "Overdraft attempt"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _ledgerService.RecordTransaction(request));
        }

        [Fact]
        public async Task GetTransactionHistory_ReturnsTransactionsInOrder()
        {
            // Arrange
            await _ledgerService.RecordTransaction(new TransactionRequest { Type = TransactionType.Deposit, Amount = 100m, Description = "First" });
            await _ledgerService.RecordTransaction(new TransactionRequest { Type = TransactionType.Deposit, Amount = 200m, Description = "Second" });

            // Act
            var result = await _ledgerService.GetTransactionHistory();

            // Assert
            Assert.Equal(2, result.TotalCount);
            Assert.Equal("Second", result.Transactions[0].Description); // Newest first
            Assert.Equal("First", result.Transactions[1].Description);
        }
    }
}