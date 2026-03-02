using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Authentication;
using ExpenseTracker.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Services;

public class GoogleSignIn: ISigInInThirdParty
{
    private const string AccessTokenKey = "thirdparty_access_token";
    private const string RefreshTokenKey = "thirdparty_refresh_token";
    private const string IdTokenKey = "thirdparty_id_token";
    private ILogger<GoogleSignIn> _logger;
    
    [SuppressMessage("ReSharper", "InconsistentNaming")] 
    private SupabaseService _supabaseService { get; }
    
    public GoogleSignIn(SupabaseService supabaseService, ILogger<GoogleSignIn> logger)
    {
        _supabaseService = supabaseService;
        _logger = logger;
    }

    public async Task<ThirdPartyAuthResult?> SignInWithGoogleAsync()
    {
        if (!AppSettings.IsSupabaseConfigured)
            throw new InvalidOperationException("Supabase is not configured. Cannot perform Google sign-in.");

        // Build Supabase OAuth authorize URL for Google
        // Supabase supports /auth/v1/authorize?provider=google&redirect_to=...
        var authorizeEndpoint = new Uri(new Uri(AppSettings.SupabaseUrl), "/auth/v1/authorize");

        // Use a redirect URI that the MAUI WebAuthenticator can handle.
        // For POC we use a custom scheme. You can later add a proper redirect in AppSettings.
        var callbackUrl = new Uri("expensetracker://auth");

        var url = new UriBuilder(authorizeEndpoint)
        {
            Query = $"provider=google&redirect_to={Uri.EscapeDataString(callbackUrl.ToString())}&response_type=code"
        }.Uri;

        // Launch the web auth flow
        var authResult = await WebAuthenticator.AuthenticateAsync(url, callbackUrl);

        // authResult contains properties from the redirect, e.g., access_token, refresh_token, etc.
        // Supabase may return a code which then needs to be exchanged; the WebAuthenticator on some platforms provides tokens directly.

        // Try to extract tokens from result properties
        authResult.Properties.TryGetValue("access_token", out var accessToken);
        authResult.Properties.TryGetValue("refresh_token", out var refreshToken);
        authResult.Properties.TryGetValue("id_token", out var idToken);

        // Persist tokens for session
        if (!string.IsNullOrEmpty(accessToken))
            await TokenStorage.SetAsync(AccessTokenKey, accessToken);
        if (!string.IsNullOrEmpty(refreshToken))
            await TokenStorage.SetAsync(RefreshTokenKey, refreshToken);
        if (!string.IsNullOrEmpty(idToken))
            await TokenStorage.SetAsync(IdTokenKey, idToken);
        var sessionSet = await SetSupabaseSessionAsync(accessToken, refreshToken);
        if (sessionSet)
        {
            return new ThirdPartyAuthResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                IdToken = idToken,
                Properties = authResult.Properties
            };
        }
        throw new AuthenticationException("Failed to set Supabase session with obtained tokens.");
    }

    public async Task SignOutAsync()
    {
        // Sign out from Supabase server-side
        try
        {
            if (_supabaseService.Client != null)
                await _supabaseService.Client.Auth.SignOut();
        }
        catch
        {
            // Best-effort server sign-out; continue clearing local state
        }

        // Clear tokens from storage
        TokenStorage.Remove(AccessTokenKey);
        TokenStorage.Remove(RefreshTokenKey);
        TokenStorage.Remove(IdTokenKey);
    }

    public ThirdPartyUserInfo? GetCurrentUserAsync()
    {
        var currentUser = _supabaseService.Client?.Auth.CurrentUser;
        var name = string.Empty;
        if (currentUser is null)
        {
            throw new AuthenticationException("No current user");
        }
        if (currentUser.UserMetadata.TryGetValue("name", out var userName))
        {
            name = Convert.ToString(userName) ?? string.Empty;
        }
        return new ThirdPartyUserInfo
        {
            Id = currentUser.Id,
            Email = currentUser.Email,
            Name = name
        };
    }

    public async Task<bool> TryRestoreSessionAsync()
    {
        try
        {
            var accessToken = await TokenStorage.GetAsync(AccessTokenKey);
            var refreshToken = await TokenStorage.GetAsync(RefreshTokenKey);

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                return false;

            return await SetSupabaseSessionAsync(accessToken, refreshToken);
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> SetSupabaseSessionAsync(string accessToken, string refreshToken)
    {
        Debug.Assert(_supabaseService.Client != null, "_supabaseService.Client != null");
        try
        {
            await _supabaseService.Client.Auth.SetSession(accessToken, refreshToken);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to set Supabase session with obtained tokens.");
            return false;
        }
    }
}