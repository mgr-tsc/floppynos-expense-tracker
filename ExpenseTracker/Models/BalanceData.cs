using Newtonsoft.Json;

namespace ExpenseTracker.Models;

public class BalanceData
{
    [JsonProperty("user_a_name")]
    public string UserAName { get; set; } = string.Empty;

    [JsonProperty("user_a_owes")]
    public decimal PersonAOwes { get; set; }

    [JsonProperty("user_b_name")]
    public string UserBName { get; set; } = string.Empty;

    [JsonProperty("user_b_owes")]
    public decimal PersonBOwes { get; set; }

    [JsonProperty("summary")]
    public string Summary { get; set; } = string.Empty;

    [Newtonsoft.Json.JsonIgnore]
    public string TotalOwed => (PersonAOwes + PersonBOwes).ToString("C");

    [Newtonsoft.Json.JsonIgnore]
    public PersonInfo PersonA => new() { Name = UserAName };

    [Newtonsoft.Json.JsonIgnore]
    public PersonInfo PersonB => new() { Name = UserBName };
}

public class PersonInfo
{
    public string Name { get; set; } = string.Empty;
}
