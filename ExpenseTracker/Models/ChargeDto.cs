using System.Text.Json.Serialization;

namespace ExpenseTracker.Models;

public class ChargeDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("user_id_fk")] public string UserIdFk { get; set; } = string.Empty; // uuid in supabase
    [JsonPropertyName("amount")] public decimal Amount { get; set; }
    [JsonPropertyName("transaction_date")] public DateTimeOffset TransactionDate { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
    [JsonPropertyName("category_id_fk")] public short CategoryIdFk { get; set; }
    [JsonPropertyName("card_id_fk")] public long CardIdFk { get; set; }
    [JsonPropertyName("split_policy")] public string SplitPolicy { get; set; } = "50-50";
}
