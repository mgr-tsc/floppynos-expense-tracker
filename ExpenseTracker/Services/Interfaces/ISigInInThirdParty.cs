namespace ExpenseTracker.Services.Interfaces;

public class ThirdPartyAuthResult
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? IdToken { get; set; }
    public IDictionary<string, string>? Properties { get; set; }
}

public class ThirdPartyUserInfo
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
}

public interface ISigInInThirdParty
{
    /// <summary>
    /// Initiates a Google sign-in flow and returns authentication tokens if successful.
    /// </summary>
    Task<ThirdPartyAuthResult?> SignInWithGoogleAsync();

    /// <summary>
    /// Signs the current user out (revokes local session / tokens as needed).
    /// </summary>
    Task SignOutAsync();

    /// <summary>
    /// Returns the currently authenticated user's basic info, or null if not signed in.
    /// </summary>
    Task<ThirdPartyUserInfo?> GetCurrentUserAsync();

    /// <summary>
    /// Attempts to restore a previous session from stored tokens.
    /// Returns true if the session was successfully restored.
    /// </summary>
    Task<bool> TryRestoreSessionAsync();
}