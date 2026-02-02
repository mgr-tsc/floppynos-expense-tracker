using System.Text.Json.Serialization;

namespace ExpenseTracker.Models;

public class Card
{
    public int ID { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public string LastFourDigits { get; set; } = string.Empty;

    [JsonIgnore] public int ProfileID { get; set; }

    public Profile? Profile { get; set; }

    [JsonIgnore]
    public string DisplayName => $"{Alias} ****{LastFourDigits}";

    public override string ToString() => DisplayName;
}
