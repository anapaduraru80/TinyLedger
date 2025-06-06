using TinyLedger.Controllers;
using TinyLedger.Models;
using TinyLedger.Services;
using Microsoft.AspNetCore.Mvc;

namespace TinyLedger.Tests.Integration
{
    public class IntegrationTests
    {
        private readonly LedgerController _controller;
        private readonly Guid _accountId = Account.DefaultAccountId;

        public IntegrationTests()
        {
            var ledgerService = new LedgerService();
            _controller = new LedgerController(ledgerService);
        }

        [Fact]
        public async Task CompleteWorkflow_DepositWithdrawQuery_WorksCorrectly()
        {
            // 1. Make a deposit
            var depositResult = await _controller.RecordTransaction(_accountId, new TransactionRequest
            {
                Type = TransactionType.Deposit,
                Amount = 1000m,
                Description = "Initial deposit"
            });
            var deposit = ((CreatedAtActionResult)depositResult.Result!).Value as Transaction;

            // 2. Check balance
            var balanceResult = await _controller.GetCurrentBalance(_accountId);
            var balance = ((OkObjectResult)balanceResult.Result!).Value as BalanceResponse;
            Assert.Equal(1000m, balance!.Balance);

            // 3. Make a withdrawal
            await _controller.RecordTransaction(_accountId, new TransactionRequest
            {
                Type = TransactionType.Withdrawal,
                Amount = 250m,
                Description = "ATM withdrawal"
            });

            // 4. Check final balance
            var finalBalanceResult = await _controller.GetCurrentBalance(_accountId);
            var finalBalance = ((OkObjectResult)finalBalanceResult.Result!).Value as BalanceResponse;
            Assert.Equal(750m, finalBalance!.Balance);

            // 5. Check transaction history
            var historyResult = await _controller.GetTransactionHistory(_accountId);
            var history = ((OkObjectResult)historyResult.Result!).Value as TransactionHistoryResponse;
            Assert.Equal(2, history!.TotalCount);

            // 6. Get specific transaction
            var transactionResult = await _controller.GetTransaction(_accountId, deposit!.Id);
            var transaction = ((OkObjectResult)transactionResult.Result!).Value as Transaction;
            Assert.Equal(deposit.Id, transaction!.Id);
        }
    }
}