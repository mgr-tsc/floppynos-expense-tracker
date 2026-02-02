namespace ExpenseTracker.Models;

public class Profile
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}
