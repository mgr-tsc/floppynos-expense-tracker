using System.Text.Json.Serialization;

namespace ExpenseTracker.Models;

public class PolicyDto
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("available")] public bool Available { get; set; } = true;
    [JsonPropertyName("label")] public string? Label { get; set; }
    [JsonPropertyName("user_a_percentage")] public short UserAPercentage { get; set; } = 50;
    [JsonPropertyName("user_b_percentage")] public short UserBPercentage { get; set; } = 50;
}
