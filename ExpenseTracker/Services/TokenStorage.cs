namespace ExpenseTracker.Services;

/// <summary>
/// Abstracts token storage to work around SecureStorage entitlement issues on macCatalyst during development.
/// Uses Preferences in DEBUG (no entitlements needed) and SecureStorage in RELEASE.
/// </summary>
public static class TokenStorage
{
    public static async Task SetAsync(string key, string value)
    {
#if DEBUG
        Preferences.Set(key, value);
        await Task.CompletedTask;
#else
        await SecureStorage.SetAsync(key, value);
#endif
    }

    public static async Task<string?> GetAsync(string key)
    {
#if DEBUG
        return await Task.FromResult(Preferences.Get(key, null as string));
#else
        return await SecureStorage.GetAsync(key);
#endif
    }

    public static void Remove(string key)
    {
#if DEBUG
        Preferences.Remove(key);
#else
        SecureStorage.Remove(key);
#endif
    }

    public static void RemoveAll()
    {
#if DEBUG
        Preferences.Clear();
#else
        SecureStorage.RemoveAll();
#endif
    }
}
