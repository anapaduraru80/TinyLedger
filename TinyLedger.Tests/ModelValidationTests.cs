using System.ComponentModel.DataAnnotations;
using TinyLedger.Models;

namespace TinyLedger.Tests.Models
{
    public class ModelValidationTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, context, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void TransactionRequest_ValidRequest_PassesValidation()
        {
            // Arrange
            var request = new TransactionRequest
            {
                Type = TransactionType.Deposit,
                Amount = 100m,
                Description = "Valid transaction"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void TransactionRequest_InvalidAmount_FailsValidation()
        {
            // Arrange
            var request = new TransactionRequest
            {
                Type = TransactionType.Deposit,
                Amount = 0m, // Invalid
                Description = "Invalid amount"
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains("Amount must be between", validationResults[0].ErrorMessage);
        }

        [Fact]
        public void TransactionRequest_DescriptionTooLong_FailsValidation()
        {
            // Arrange
            var request = new TransactionRequest
            {
                Type = TransactionType.Deposit,
                Amount = 100m,
                Description = new string('a', 501) // Too long
            };

            // Act
            var validationResults = ValidateModel(request);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains("Description cannot exceed 500 characters", validationResults[0].ErrorMessage);
        }
    }
}