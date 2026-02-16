using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ExpenseTracker.Models.Supabase;

[Table("HOUSEHOLD")]
public class HouseHoldDto: BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [Column("code")]
    public long Code { get; set; }

    [Column("enabled")]
    public bool Enabled { get; set; }

    [Column("user_a_id_fk")]
    public string UserAIdFk { get; set; }

    [Column("user_b_id_fk")]
    public string UserBIdFk { get; set; }
}
