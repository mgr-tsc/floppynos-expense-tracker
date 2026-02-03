using System.Text.Json.Serialization;

namespace ExpenseTracker.Models;

public class Charge
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Today;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string SplitPolicy { get; set; } = "50/50";
    
    [JsonIgnore] 
    public int CardId { get; set; }
    [JsonIgnore] 
    public int PayerProfileId { get; set; }

    public Card? Card { get; set; }
    public Profile? Payer { get; set; }

    [JsonIgnore]
    public string DisplayDate => Date.ToString("MMM d, yyyy");

    [JsonIgnore]
    public string DisplayAmount => Amount.ToString("C2");
}
