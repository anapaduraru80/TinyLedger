# Tiny Ledger API

This is a simple in-memory ledger API built with ASP.NET Core, designed as a take-home assignment. It allows recording deposits and withdrawals, viewing the current balance, and retrieving transaction history for a single account.

## Features

* **Deposit**: Record money coming into the account.
* **Withdrawal**: Record money leaving the account.
* **Current Balance**: View the current balance of the account.
* **Transaction History**: View a paginated list of all recorded transactions.
* **Individual Transaction**: Retrieve details of a specific transaction.
* **Account Information**: View account details including IBAN and transaction count.

## Technical Details & Assumptions

* **Technology Stack**: C# and ASP.NET Core Web API 8.0.
* **Data Storage**: All data (transactions and balance) is stored in-memory using static data structures. This means data will be lost when the application restarts.
* **Single Account**: The system manages one fixed account with a predetermined IBAN (ES9121000418450200051332) and account ID.
* **No Persistence**: As per requirements, there is no database or file persistence.
* **No Authentication/Authorization**: All API endpoints are publicly accessible.
* **Thread Safety**: Basic thread safety is implemented using lock statements for concurrent access.
* **Fixed Account ID**: All endpoints require the specific account ID `12345678-1234-5678-9abc-123456789012` in the URL path.
* **Error Handling**: Comprehensive validation for amounts, account validation, and sufficient balance checks.
* **API Versioning**: All responses include an API-Version header set to "1.0".

## How to Run

1.  **Prerequisites**:
    * .NET SDK 8.0 or later installed. You can download it from [dot.net](https://dotnet.microsoft.com/download).

2.  **Clone the Repository**:
    ```bash
    git clone https://github.com/anapaduraru80/TinyLedger
    cd TinyLedger
    ```

3.  **Run the Application**:
    Navigate to the project root directory in your terminal and run:
    ```bash
    dotnet run
    ```
    The application will typically start on `https://localhost:7080` (or another port assigned by ASP.NET Core). The exact URL will be displayed in the console output.

    You can also run it via Visual Studio or VS Code if you have them installed.

## Running Tests

The project includes a comprehensive test suite in the `TinyLedger.Tests` project to ensure reliability and correctness of the API.

### How to Run Tests

Navigate to the solution root directory and use any of the following commands:

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests with code coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "ClassName=LedgerServiceTests"

# Run tests in watch mode (reruns on file changes)
dotnet watch test --project TinyLedger.Tests
```

### Test Coverage

The test suite includes **14 focused tests** covering the essential functionality:

#### 1. **Service Layer Tests** (`LedgerServiceTests.cs`) - 4 tests
- **Deposit Operations**: Verifies deposits correctly increase account balance
- **Withdrawal Operations**: Verifies withdrawals correctly decrease account balance
- **Insufficient Funds**: Ensures withdrawals are rejected when balance is insufficient
- **Transaction History**: Validates transactions are returned in correct chronological order (newest first)

#### 2. **Controller Layer Tests** (`LedgerControllerTests.cs`) - 4 tests
- **Valid Account Access**: Confirms valid account ID returns HTTP 200 OK
- **Invalid Account Handling**: Ensures invalid account ID returns HTTP 404 Not Found
- **Successful Transactions**: Validates successful deposits return HTTP 201 Created
- **Error Responses**: Confirms insufficient funds return HTTP 409 Conflict

#### 3. **Model Validation Tests** (`ModelValidationTests.cs`) - 3 tests
- **Valid Requests**: Ensures properly formatted requests pass validation
- **Amount Validation**: Confirms invalid amounts (≤0 or >999,999,999.99) are rejected
- **Description Validation**: Ensures descriptions over 500 characters are rejected

#### 4. **Integration Tests** (`IntegrationTests.cs`) - 1 test
- **End-to-End Workflow**: Tests complete user journey:
  1. Make a deposit
  2. Check balance
  3. Make a withdrawal
  4. Verify final balance
  5. Review transaction history
  6. Retrieve specific transaction

#### 5. **Thread Safety Tests** (`ThreadSafetyTests.cs`) - 2 tests
- **Concurrent Deposits**: Verifies multiple simultaneous deposits maintain correct balance
- **Concurrent Withdrawals**: Ensures race conditions don't allow overdrafts

### Test Architecture

The tests follow the **Arrange-Act-Assert** pattern and cover:

- ✅ **Happy Path Scenarios**: Normal operation flows
- ✅ **Error Conditions**: Invalid inputs and business rule violations
- ✅ **Edge Cases**: Boundary conditions and validation limits
- ✅ **HTTP Status Codes**: Correct response codes for different scenarios
- ✅ **Thread Safety**: Concurrent access patterns
- ✅ **Data Integrity**: Balance calculations and transaction ordering

## API Endpoints and Examples

Once the application is running, you can access the Swagger UI at `https://localhost:7080/swagger` (replace port if different) to explore the API endpoints interactively.

**Important**: All endpoints require the fixed account ID `12345678-1234-5678-9abc-123456789012` in the URL path.

Base URL: `https://localhost:7080/api/accounts/12345678-1234-5678-9abc-123456789012`

*(Note: When using `curl` with `https://localhost`, you might need to use the `-k` or `--insecure` flag to bypass SSL certificate validation warnings, as the development certificate is self-signed.)*

### 1. Get Account Information

* **Endpoint**: `GET /api/accounts/{accountId}`
* **Description**: Retrieves account details including IBAN, balance, and transaction count.

**Example Request (curl):**
```bash
curl -X GET "https://localhost:7080/api/accounts/12345678-1234-5678-9abc-123456789012" -k
```

**Example Response:**
```json
{
  "id": "12345678-1234-5678-9abc-123456789012",
  "iban": "ES9121000418450200051332",
  "currentBalance": 1250.75,
  "createdAt": "2024-01-15T08:00:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "transactionCount": 5
}
```

### 2. View Current Balance

* **Endpoint**: `GET /api/accounts/{accountId}/balance`
* **Description**: Retrieves the current balance of the account.

**Example Request (curl):**
```bash
curl -X GET "https://localhost:7080/api/accounts/12345678-1234-5678-9abc-123456789012/balance" -k
```

**Example Response:**
```json
{
  "balance": 1250.75,
  "asOf": "2024-01-15T10:30:00Z",
  "accountId": "12345678-1234-5678-9abc-123456789012",
  "accountIBAN": "ES9121000418450200051332"
}
```

### 3. Record a Transaction (Deposit or Withdrawal)

* **Endpoint**: `POST /api/accounts/{accountId}/transactions`
* **Description**: Records a deposit or withdrawal transaction.

**Request Body:**
```json
{
  "type": "deposit",  // "deposit" or "withdrawal"
  "amount": 100.50,
  "description": "Salary payment"
}
```

**Example Request - Deposit (curl):**
```bash
curl -X POST "https://localhost:7080/api/accounts/12345678-1234-5678-9abc-123456789012/transactions" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "deposit",
    "amount": 500.00,
    "description": "Initial deposit"
  }' -k
```

**Example Request - Withdrawal (curl):**
```bash
curl -X POST "https://localhost:7080/api/accounts/12345678-1234-5678-9abc-123456789012/transactions" \
  -H "Content-Type: application/json" \
  -d '{
    "type": "withdrawal",
    "amount": 50.25,
    "description": "ATM withdrawal"
  }' -k
```

**Example Response:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "accountId": "12345678-1234-5678-9abc-123456789012",
  "type": "deposit",
  "amount": 500.00,
  "timestamp": "2024-01-15T10:30:00Z",
  "description": "Initial deposit",
  "balanceAfterTransaction": 1750.75
}
```

### 4. View Transaction History

* **Endpoint**: `GET /api/accounts/{accountId}/transactions`
* **Description**: Retrieves paginated transaction history, ordered by timestamp (newest first).
* **Query Parameters**:
  - `page` (optional): Page number (default: 1, must be > 0)
  - `pageSize` (optional): Number of transactions per page (default: 50, range: 1-100)

**Example Request (curl):**
```bash
curl -X GET "https://localhost:7080/api/accounts/12345678-1234-5678-9abc-123456789012/transactions?page=1&pageSize=10" -k
```

**Example Response:**
```json
{
  "transactions": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "accountId": "12345678-1234-5678-9abc-123456789012",
      "type": "deposit",
      "amount": 500.00,
      "timestamp": "2024-01-15T10:30:00Z",
      "description": "Initial deposit",
      "balanceAfterTransaction": 1750.75
    },
    {
      "id": "987fcdeb-51a2-43d1-9f12-426614174001",
      "accountId": "12345678-1234-5678-9abc-123456789012",
      "type": "withdrawal",
      "amount": 50.25,
      "timestamp": "2024-01-15T09:15:00Z",
      "description": "ATM withdrawal",
      "balanceAfterTransaction": 1250.75
    }
  ],
  "totalCount": 25,
  "page": 1,
  "pageSize": 10,
  "accountId": "12345678-1234-5678-9abc-123456789012"
}
```

### 5. Get Specific Transaction

* **Endpoint**: `GET /api/accounts/{accountId}/transactions/{transactionId}`
* **Description**: Retrieves details of a specific transaction by its ID.

**Example Request (curl):**
```bash
curl -X GET "https://localhost:7080/api/accounts/12345678-1234-5678-9abc-123456789012/transactions/123e4567-e89b-12d3-a456-426614174000" -k
```

**Example Response:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "accountId": "12345678-1234-5678-9abc-123456789012",
  "type": 0,
  "amount": 500.00,
  "timestamp": "2024-01-15T10:30:00Z",
  "description": "Initial deposit",
  "balanceAfterTransaction": 1750.75
}
```

### 6. Health Check

* **Endpoint**: `GET /api/accounts/{accountId}/health`
* **Description**: Simple health check endpoint for the specific account.

**Example Request (curl):**
```bash
curl -X GET "https://localhost:7080/api/accounts/12345678-1234-5678-9abc-123456789012/health" -k
```

**Example Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2024-01-15T10:30:00Z",
  "accountId": "12345678-1234-5678-9abc-123456789012"
}
```

## Transaction Types

The API uses numeric values for transaction types:
- **0**: Deposit
- **1**: Withdrawal

## Error Handling

The API returns appropriate HTTP status codes and error messages:

- **400 Bad Request**: Invalid input data (negative amounts, invalid pagination parameters, validation errors)
- **404 Not Found**: Invalid account ID or transaction not found
- **409 Conflict**: Insufficient balance for withdrawal
- **500 Internal Server Error**: Unexpected server errors

**Example Error Response:**
```json
{
  "message": "Insufficient balance for withdrawal.",
  "details": null,
  "statusCode": 409
}
```

**Example Validation Error Response:**
```json
{
  "message": "Invalid request data",
  "details": "Amount is required; Amount must be between 0.01 and 999,999,999.99",
  "statusCode": 400
}
```

## Testing the API

You can test the complete workflow using the fixed account ID `12345678-1234-5678-9abc-123456789012`:

1. **Check account information** (should show initial state)
2. **Check initial balance** (should be 0.00)
3. **Make a deposit** (e.g., $1000)
4. **Check balance again** (should be $1000)
5. **Make a withdrawal** (e.g., $200)
6. **Check balance** (should be $800)
7. **View transaction history** (should show both transactions)
8. **Get specific transaction** (using transaction ID from previous responses)
9. **Try to withdraw more than available** (should get 409 error)
10. **Try with invalid account ID** (should get 404 error)

## Architecture Notes

* **Controller Layer**: Handles HTTP requests/responses, validation, and account ID verification
* **Service Layer**: Contains business logic for transaction processing and account management
* **Model Layer**: Defines data structures, validation rules, and response models
* **In-Memory Storage**: Simple static collections with thread safety using lock statements
* **Single Account Design**: All operations are performed on one fixed account with predetermined IBAN
* **API Versioning**: Automatic API version header injection through custom filter
