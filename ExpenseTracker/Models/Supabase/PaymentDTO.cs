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
    
    [Column("charge_id_fk")]
    public long ChargeIdFk { get; set; }
    
    // Navigation properties
    [Reference(typeof(ChargeDto))]
    public ChargeDto PaymentCharge { get; set; }
    
}