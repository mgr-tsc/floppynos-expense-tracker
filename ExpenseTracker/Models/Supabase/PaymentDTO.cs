using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ExpenseTracker.Models.Supabase;

[Table("PAYMENT")]
public class PaymentDTO : BaseModel
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
    [Reference(typeof(ChargeDTO))]
    public ChargeDTO PaymentCharge { get; set; }
    
}