using System.Text.Json;

using System.Text.Json.Serialization;

namespace TinyLedger.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TransactionType
    {
        Deposit = 0,
        Withdrawal = 1
    }
}