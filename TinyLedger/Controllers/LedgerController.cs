using Microsoft.AspNetCore.Mvc;
using TinyLedger.Models;
using TinyLedger.Services;

namespace TinyLedger.Controllers
{
    [ApiController]
    [Route("api/accounts/{accountId:guid}")]
    public class LedgerController : ControllerBase
    {
        private readonly ILedgerService _ledgerService;

        public LedgerController(ILedgerService ledgerService)
        {
            _ledgerService = ledgerService;
        }

        /// <summary>
        /// Validates that the account ID in the route matches the expected fixed account ID.
        /// </summary>
        private bool ValidateAccountId(Guid accountId)
        {
            return accountId == Account.DefaultAccountId;
        }

        /// <summary>
        /// Get account information.
        /// </summary>
        /// <param name="accountId">The account ID (must match the fixed account ID).</param>
        /// <returns>Account details.</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AccountResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<AccountResponse>> GetAccount(Guid accountId)
        {
            if (!ValidateAccountId(accountId))
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Account not found",
                    StatusCode = 404
                });
            }

            var account = await _ledgerService.GetAccount();
            return Ok(account);
        }

        /// <summary>
        /// Retrieves the current balance of the account.
        /// </summary>
        /// <param name="accountId">The account ID (must match the fixed account ID).</param>
        /// <returns>The current balance as a decimal.</returns>
        [HttpGet("balance")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BalanceResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<BalanceResponse>> GetCurrentBalance(Guid accountId)
        {
            if (!ValidateAccountId(accountId))
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Account not found",
                    StatusCode = 404
                });
            }

            var balance = await _ledgerService.GetBalance();
            return Ok(balance);
        }

        /// <summary>
        /// Get transaction history with pagination.
        /// </summary>
        /// <param name="accountId">The account ID (must match the fixed account ID).</param>
        /// <param name="page">Page number (default: 1).</param>
        /// <param name="pageSize">Page size (default: 50, max: 100).</param>
        /// <returns>Transaction history for the account.</returns>
        [HttpGet("transactions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionHistoryResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<TransactionHistoryResponse>> GetTransactionHistory(
            Guid accountId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            if (!ValidateAccountId(accountId))
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Account not found",
                    StatusCode = 404
                });
            }

            if (page < 1)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = "Page number must be greater than 0",
                    StatusCode = 400
                });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = "Page size must be between 1 and 100",
                    StatusCode = 400
                });
            }

            var history = await _ledgerService.GetTransactionHistory(page, pageSize);
            return Ok(history);
        }

        /// <summary>
        /// Get a specific transaction by ID.
        /// </summary>
        /// <param name="accountId">The account ID (must match the fixed account ID).</param>
        /// <param name="transactionId">The transaction ID.</param>
        /// <returns>The transaction details.</returns>
        [HttpGet("transactions/{transactionId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Transaction))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Transaction>> GetTransaction(Guid accountId, Guid transactionId)
        {
            if (!ValidateAccountId(accountId))
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Account not found",
                    StatusCode = 404
                });
            }

            var transaction = await _ledgerService.GetTransaction(transactionId);
            if (transaction == null)
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Transaction not found",
                    StatusCode = 404
                });
            }

            return Ok(transaction);
        }

        /// <summary>
        /// Records a deposit or withdrawal into the account.
        /// </summary>
        /// <param name="accountId">The account ID (must match the fixed account ID).</param>
        /// <param name="request">The request containing amount, description, and transaction type.</param>
        /// <returns>The created transaction object.</returns>
        [HttpPost("transactions")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Transaction))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorResponse))]
        public async Task<ActionResult<Transaction>> RecordTransaction(
            Guid accountId,
            [FromBody] TransactionRequest request)
        {
            if (!ValidateAccountId(accountId))
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Account not found",
                    StatusCode = 404
                });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = "Invalid request data",
                    Details = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)),
                    StatusCode = 400
                });
            }

            try
            {
                var transaction = await _ledgerService.RecordTransaction(request);
                return CreatedAtAction(nameof(GetTransaction),
                    new { accountId, transactionId = transaction.Id }, transaction);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse
                {
                    Message = ex.Message,
                    StatusCode = 400
                });
            }
            catch (InvalidOperationException ex) // Insufficient funds
            {
                return Conflict(new ErrorResponse
                {
                    Message = ex.Message,
                    StatusCode = 409
                });
            }
        }

        /// <summary>
        /// Health check endpoint.
        /// </summary>
        /// <param name="accountId">The account ID (must match the fixed account ID).</param>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorResponse))]
        public IActionResult Health(Guid accountId)
        {
            if (!ValidateAccountId(accountId))
            {
                return NotFound(new ErrorResponse
                {
                    Message = "Account not found",
                    StatusCode = 404
                });
            }

            return Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow, AccountId = accountId });
        }
    }
}