using System.ComponentModel.DataAnnotations;

namespace TinyLedger.Models
{
    public class TransactionRequest
    {
        [Required(ErrorMessage = "Transaction type is required")]
        [EnumDataType(typeof(TransactionType), ErrorMessage = "Invalid transaction type")]
        public TransactionType Type { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, 999999999.99, ErrorMessage = "Amount must be between 0.01 and 999,999,999.99")]
        public decimal Amount { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = string.Empty;
    }
}
