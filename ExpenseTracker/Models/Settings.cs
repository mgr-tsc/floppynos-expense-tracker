using System.Text.Json.Serialization;

namespace ExpenseTracker.Models;

public class Settings
{
    [JsonPropertyName("VersionApp")]
    public string VersionApp { get; set; } = string.Empty;
}