namespace TinyLedger.Models
{
    public class ErrorResponse
    {
        public string? Message { get; set; }
        public string? Details { get; set; }
        public int StatusCode { get; set; }
    }
}
