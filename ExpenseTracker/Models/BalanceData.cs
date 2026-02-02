namespace ExpenseTracker.Models;

public class BalanceData
{
    public Profile? PersonA { get; set; }
    public Profile? PersonB { get; set; }
    public decimal PersonAOwes { get; set; }
    public decimal PersonBOwes { get; set; }
    public decimal NetBalance { get; set; }

    public string Summary
    {
        get
        {
            if (NetBalance == 0)
                return "All settled up!";

            if (NetBalance > 0)
                return $"{PersonB?.Name} owes {PersonA?.Name} {NetBalance:C2}";

            return $"{PersonA?.Name} owes {PersonB?.Name} {Math.Abs(NetBalance):C2}";
        }
    }
}
