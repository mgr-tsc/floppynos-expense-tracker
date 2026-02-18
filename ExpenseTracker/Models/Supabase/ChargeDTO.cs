using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ExpenseTracker.Models.Supabase;

[Table("CHARGE")]
public class ChargeDto : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("transaction_date")]
    public DateTime? TransactionDate { get; set; }

    [Column("category_id_fk")]
    public short CategoryIdFk { get; set; }

    [Column("card_id_fk")]
    public long CardIdFk { get; set; }

    [Column("policy_id_fk")]
    public long PolicyIdFk { get; set; }

    [Column("user_id_fk")]
    public string UserIdFk { get; set; }

    [Column("household_id_fk")]
    public long HouseholdIdFk { get; set; }

    [Column("status")]
    public string Status { get; set; } = "pending";

    // Navigation properties
    [Reference(typeof(PolicyDto))]
    public PolicyDto ChargePolicy { get; set; }

    [Reference(typeof(ChargeCategoryDto))]
    public ChargeCategoryDto ChargeCategory { get; set; }

    [Reference(typeof(CardDto))]
    public CardDto ChargeCard { get; set; }

    // Computed display properties
    [Newtonsoft.Json.JsonIgnore]
    public string DisplayAmount => Amount.ToString("C");

    [Newtonsoft.Json.JsonIgnore]
    public string DisplayDate => TransactionDate?.ToString("MMM d") ?? "";

    [Newtonsoft.Json.JsonIgnore]
    public string SplitPolicy => ChargePolicy?.DisplayLabel ?? "";

    [Newtonsoft.Json.JsonIgnore]
    public string StatusLabel => string.IsNullOrEmpty(Status)
        ? ""
        : char.ToUpper(Status[0]) + Status[1..];

    [Newtonsoft.Json.JsonIgnore]
    public Color StatusColor => Status?.ToLower() switch
    {
        "approved" => Colors.Green,
        "rejected" => Colors.Red,
        _ => Colors.Orange,
    };

    public bool IsOwnCharge(string currentUserId) => UserIdFk == currentUserId;
}
