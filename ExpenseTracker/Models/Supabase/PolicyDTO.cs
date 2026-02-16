using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ExpenseTracker.Models.Supabase;

[Table("POLICY")]
public class PolicyDto: BaseModel
{
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("available")]
    public bool Available { get; set; }

    [Column("label")]
    public string? Label { get; set; }

    [Column("user_a_percentage")]
    public short UserAPercentage { get; set; }

    [Column("user_b_percentage")]
    public short UserBPercentage { get; set; }
}
