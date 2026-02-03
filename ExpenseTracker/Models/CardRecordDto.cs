using System;
using System.Text.Json.Serialization;

namespace ExpenseTracker.Models;

public class CardRecordDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("last_digits")] public int LastDigits { get; set; }
    [JsonPropertyName("symbol")] public string Symbol { get; set; } = string.Empty;
}
