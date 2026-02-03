using System.Diagnostics.CodeAnalysis;

namespace ExpenseTracker.Utilities;

public static class ModelExtensions
{
    public static bool IsNullOrNew([NotNullWhen(false)] this Profile? profile)
    {
        return profile is null || profile.Id == 0;
    }

    public static bool IsNullOrNew([NotNullWhen(false)] this Card? card)
    {
        return card is null || card.Id == 0;
    }

    public static bool IsNullOrNew([NotNullWhen(false)] this Charge? expense)
    {
        return expense is null || expense.Id == 0;
    }
}


