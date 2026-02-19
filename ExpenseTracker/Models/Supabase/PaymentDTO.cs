using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ExpenseTracker.Models.Supabase;

[Table("PAYMENT")]
public class PaymentDto : BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("amount")]
    public decimal Amount { get; set; }

    [Column("payment_date")]
    public DateTime? PaymentDate { get; set; }

    [Column("user_id_fk")]
    public string UserIdFk { get; set; }

    [Column("household_id_fk")]
    public long HouseholdIdFk { get; set; }

    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("payment_method")]
    public string PaymentMethod { get; set; } = "cash";

    // Computed display properties
    [Newtonsoft.Json.JsonIgnore]
    public string DisplayAmount => Amount.ToString("C");

    [Newtonsoft.Json.JsonIgnore]
    public string DisplayDate => PaymentDate?.ToString("MMM d") ?? "";

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

    [Newtonsoft.Json.JsonIgnore]
    public string PaymentMethodLabel => PaymentMethod switch
    {
        "transfer_paypal" => "PayPal",
        "transfer_zelle" => "Zelle",
        "transfer_applepay" => "Apple Pay",
        "check" => "Check",
        _ => "Cash",
    };

    public bool IsOwnPayment(string currentUserId) => UserIdFk == currentUserId;
}
