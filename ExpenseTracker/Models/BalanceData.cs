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

    // Set by the app after deserialization — true if the current viewer is user_a in the household
    public bool CurrentUserIsA { get; set; } = true;

    [JsonIgnore]
    private string PartnerName => CurrentUserIsA ? UserBName : UserAName;

    // Positive  = viewer owes partner
    // Negative  = partner owes viewer
    // Zero      = settled
    [JsonIgnore]
    private decimal SignedNet => CurrentUserIsA
        ? PersonAOwes - PersonBOwes
        : PersonBOwes - PersonAOwes;

    [JsonIgnore]
    public string NetAmountFormatted => SignedNet < 0
        ? $"-{Math.Abs(SignedNet):C}"
        : SignedNet.ToString("C");

    [JsonIgnore]
    public string BalanceNote
    {
        get
        {
            var absValue = Math.Abs(SignedNet).ToString("C");
            if (SignedNet == 0) return "All settled up!";
            return SignedNet > 0
                ? $"You owe {PartnerName} {absValue}"
                : $"{PartnerName} owes you {absValue}";
        }
    }
}
