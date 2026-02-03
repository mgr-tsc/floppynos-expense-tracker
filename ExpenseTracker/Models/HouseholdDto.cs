using System.Text.Json.Serialization;

namespace ExpenseTracker.Models;

public class HouseholdDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("created_at")] public DateTimeOffset CreatedAt { get; set; }
    [JsonPropertyName("user_a")] public string? UserA { get; set; }
    [JsonPropertyName("user_b")] public string? UserB { get; set; }
    [JsonPropertyName("code")] public int Code { get; set; }
    [JsonPropertyName("enabled")] public bool Enabled { get; set; } = true;
}
