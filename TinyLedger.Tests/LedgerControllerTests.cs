using Microsoft.AspNetCore.Mvc;
using TinyLedger.Controllers;
using TinyLedger.Models;
using TinyLedger.Services;

namespace TinyLedger.Tests.Controllers
{
    public class LedgerControllerTests
    {
        private readonly LedgerController _controller;
        private readonly Guid _validAccountId = Account.DefaultAccountId;
        private readonly Guid _invalidAccountId = Guid.NewGuid();

        public LedgerControllerTests()
        {
            var ledgerService = new LedgerService();
            _controller = new LedgerController(ledgerService);
        }

        [Fact]
        public async Task GetAccount_ValidAccountId_ReturnsOk()
        {
            // Act
            var result = await _controller.GetAccount(_validAccountId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var account = Assert.IsType<AccountResponse>(okResult.Value);
            Assert.Equal(_validAccountId, account.Id);
        }

        [Fact]
        public async Task GetAccount_InvalidAccountId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetAccount(_invalidAccountId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            var error = Assert.IsType<ErrorResponse>(notFoundResult.Value);
            Assert.Equal("Account not found", error.Message);
        }

        [Fact]
        public async Task RecordTransaction_ValidDeposit_ReturnsCreated()
        {
            // Arrange
            var request = new TransactionRequest
            {
                Type = TransactionType.Deposit,
                Amount = 100m,
                Description = "Test deposit"
            };

            // Act
            var result = await _controller.RecordTransaction(_validAccountId, request);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var transaction = Assert.IsType<Transaction>(createdResult.Value);
            Assert.Equal(100m, transaction.Amount);
        }

        [Fact]
        public async Task RecordTransaction_InsufficientFunds_ReturnsConflict()
        {
            // Arrange
            var request = new TransactionRequest
            {
                Type = TransactionType.Withdrawal,
                Amount = 100m,
                Description = "Overdraft"
            };

            // Act
            var result = await _controller.RecordTransaction(_validAccountId, request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result.Result);
            var error = Assert.IsType<ErrorResponse>(conflictResult.Value);
            Assert.Equal("Insufficient balance for withdrawal.", error.Message);
        }
    }
}