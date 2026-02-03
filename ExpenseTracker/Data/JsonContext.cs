using System.Text.Json.Serialization;

[JsonSerializable(typeof(Profile))]
[JsonSerializable(typeof(Card))]
[JsonSerializable(typeof(Charge))]
[JsonSerializable(typeof(CardRecordDto))]
[JsonSerializable(typeof(ChargeDto))]
[JsonSerializable(typeof(ChargeCatDto))]
[JsonSerializable(typeof(HouseholdDto))]
[JsonSerializable(typeof(PolicyDto))]
public partial class JsonContext : JsonSerializerContext
{
    
}
