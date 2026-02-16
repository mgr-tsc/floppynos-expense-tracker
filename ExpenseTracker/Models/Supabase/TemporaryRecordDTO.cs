using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace ExpenseTracker.Models.Supabase;

[Table("TEMPORARY_RECORDS")]
public class TemporaryRecordDto: BaseModel
{
    [PrimaryKey("id")]
    public int Id { get; set; }
    
    [Column("created_at")]
    public DateTimeOffset CreatedAt { get; set; }
    
    [Column("expire_at")]
    public DateTimeOffset ExpireAt { get; set; }
    
    [Column("key")]
    public string Key { get; set; }
    
    [Column("record")]
    public string? Record { get; set; }
    
}