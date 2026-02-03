using System.Text.Json.Serialization;

namespace ExpenseTracker.Models;

public class Card
{
    public int Id { get; set; }
    [JsonPropertyName("supabase_id")]
    public long? SupabaseId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public string LastFourDigits { get; set; } = string.Empty;

    [JsonIgnore] public int ProfileId { get; set; }

    public Profile? Profile { get; set; }

    [JsonIgnore]
    public string DisplayName => $"{Alias} ****{LastFourDigits}";

    public override string ToString() => DisplayName;
}
