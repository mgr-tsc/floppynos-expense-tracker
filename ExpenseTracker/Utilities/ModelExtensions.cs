using System.Diagnostics.CodeAnalysis;
using ExpenseTracker.Models;

namespace ExpenseTracker.Utilities;

public static class ModelExtensions
{
    public static bool IsNullOrNew([NotNullWhen(false)] this Profile? profile)
    {
        return profile is null || profile.ID == 0;
    }

    public static bool IsNullOrNew([NotNullWhen(false)] this Card? card)
    {
        return card is null || card.ID == 0;
    }

    public static bool IsNullOrNew([NotNullWhen(false)] this Expense? expense)
    {
        return expense is null || expense.ID == 0;
    }
}
