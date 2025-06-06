using TinyLedger.Models;
using TinyLedger.Services;

namespace TinyLedger.Tests.Concurrency
{
    public class ThreadSafetyTests
    {
        [Fact]
        public async Task ConcurrentDeposits_MaintainCorrectBalance()
        {
            // Arrange
            var ledgerService = new LedgerService();
            const int numberOfThreads = 5;
            const decimal amountPerDeposit = 100m;
            var tasks = new List<Task>();

            // Act - Execute multiple deposits concurrently
            for (int i = 0; i < numberOfThreads; i++)
            {
                var task = Task.Run(async () =>
                {
                    await ledgerService.RecordTransaction(new TransactionRequest
                    {
                        Type = TransactionType.Deposit,
                        Amount = amountPerDeposit,
                        Description = "Concurrent deposit"
                    });
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Assert
            var balance = await ledgerService.GetBalance();
            var expectedBalance = numberOfThreads * amountPerDeposit;
            Assert.Equal(expectedBalance, balance.Balance);
        }

        [Fact]
        public async Task ConcurrentInsufficientFundsWithdrawals_OnlyCorrectNumberSucceed()
        {
            // Arrange
            var ledgerService = new LedgerService();
            await ledgerService.RecordTransaction(new TransactionRequest
            {
                Type = TransactionType.Deposit,
                Amount = 200m,
                Description = "Initial deposit"
            });

            var tasks = new List<Task>();
            var exceptions = new List<Exception>();

            // Act - Try multiple withdrawals of 100 each (only 2 should succeed)
            for (int i = 0; i < 5; i++)
            {
                var task = Task.Run(async () =>
                {
                    try
                    {
                        await ledgerService.RecordTransaction(new TransactionRequest
                        {
                            Type = TransactionType.Withdrawal,
                            Amount = 100m,
                            Description = "Concurrent withdrawal"
                        });
                    }
                    catch (InvalidOperationException ex)
                    {
                        lock (exceptions) { exceptions.Add(ex); }
                    }
                });
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Assert
            var finalBalance = await ledgerService.GetBalance();
            Assert.True(finalBalance.Balance >= 0); // Should never go negative
            Assert.True(exceptions.Count >= 3); // At least 3 should fail
        }
    }
}