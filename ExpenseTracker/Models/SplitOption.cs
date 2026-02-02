namespace ExpenseTracker.Models;

public class SplitOption
{
    public string Label { get; set; } = string.Empty;
    public int PayerPercent { get; set; }
    public int OtherPercent { get; set; }

    public override string ToString() => Label;

    public static List<SplitOption> Presets =>
    [
        new() { Label = "50/50", PayerPercent = 50, OtherPercent = 50 },
        new() { Label = "70/30", PayerPercent = 70, OtherPercent = 30 },
        new() { Label = "100/0", PayerPercent = 100, OtherPercent = 0 }
    ];
}
