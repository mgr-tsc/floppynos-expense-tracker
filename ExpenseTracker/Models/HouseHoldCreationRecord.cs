namespace ExpenseTracker.Models;

public class HouseHoldCreationRecord
{
    public required string HouseHoldName { get; set; }
    public required string Uuid { get; set; }
    public required string CodeToJoin { get; set; }
    public required string UserCreationEmail { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}