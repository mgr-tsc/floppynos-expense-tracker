using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ExpenseTracker.Models.Supabase;

[Table("CHARGE_CATEGORY")]
public class ChargeCategoryDto: BaseModel
{
    [PrimaryKey("id")]
    public short Id { get; set; }

    [Column("category_name")]
    public string Name { get; set; }

    public override string ToString() => Name;
}
