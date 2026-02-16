using System.Text.Json.Serialization;
using ExpenseTracker.Models.Supabase;

namespace ExpenseTracker.Data;

[JsonSerializable(typeof(CardDto))]
[JsonSerializable(typeof(ChargeDto))]
[JsonSerializable(typeof(ChargeCategoryDto))]
[JsonSerializable(typeof(HouseHoldDto))]
[JsonSerializable(typeof(PolicyDto))]
public partial class JsonContext : JsonSerializerContext
{
    
}