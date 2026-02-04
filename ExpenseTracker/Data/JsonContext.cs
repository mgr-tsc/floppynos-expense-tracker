using System.Text.Json.Serialization;
using ExpenseTracker.Models.Supabase;

namespace ExpenseTracker.Data;

[JsonSerializable(typeof(CardDTO))]
[JsonSerializable(typeof(ChargeDTO))]
[JsonSerializable(typeof(ChargeCategoryDTO))]
[JsonSerializable(typeof(HouseHoldDTO))]
[JsonSerializable(typeof(PolicyDTO))]
public partial class JsonContext : JsonSerializerContext
{
    
}