using Newtonsoft.Json;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ExpenseTracker.Models.Supabase;

[Table("CARD_RECORD")]
public class CardDto: BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("last_digits")]
    public int LastDigits { get; set; }

    [Column("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [Column("user_id_fk")]
    public string UserIdFk { get; set; } = string.Empty;

    // Computed properties for display (not stored in DB)
    [JsonIgnore]
    public string DisplayName => $"{Symbol} {Name} ••{LastDigits:D4}";

    [JsonIgnore]
    public string NetworkBadge => Symbol.ToUpperInvariant() switch
    {
        "VISA" => "VISA",
        "MASTERCARD" => "MC",
        "AMERICAN EXPRESS" => "AMEX",
        "DISCOVER" => "DISC",
        _ => Symbol.Length > 4 ? Symbol[..4].ToUpperInvariant() : Symbol.ToUpperInvariant()
    };

    [JsonIgnore]
    public string MaskedDigits => $"•••• {LastDigits:D4}";

    public override string ToString() => DisplayName;
}
