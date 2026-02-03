using System.Text.Json.Serialization;

namespace ExpenseTracker.Models;

public class ChargeCatDto
{
    [JsonPropertyName("id")] public short Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}
