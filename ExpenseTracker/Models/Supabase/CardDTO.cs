using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ExpenseTracker.Models.Supabase;

[Table("CARD_RECORD")]
public class CardDTO: BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("last_digits")]
    public int LastDigits { get; set; }

    [Column("symbol")]
    public string Symbol { get; set; }
}
