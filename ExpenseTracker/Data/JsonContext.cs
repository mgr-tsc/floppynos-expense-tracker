using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using ExpenseTracker.Models;

[JsonSerializable(typeof(Profile))]
[JsonSerializable(typeof(Card))]
[JsonSerializable(typeof(Expense))]
[JsonSerializable(typeof(SeedData))]
[JsonSerializable(typeof(SeedCard))]
[JsonSerializable(typeof(SeedExpense))]
public partial class JsonContext : JsonSerializerContext
{
    public JsonContext(JsonSerializerOptions? options) : base(options)
    {
        
    }

    public override JsonTypeInfo? GetTypeInfo(Type type)
    {
        throw new NotImplementedException();
    }

    protected override JsonSerializerOptions? GeneratedSerializerOptions { get; }
}
