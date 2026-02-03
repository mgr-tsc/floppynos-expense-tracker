using System.Text.Json;
using ExpenseTracker.Services.Interfaces;
using ExpenseTracker.Data;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Storage;

namespace ExpenseTracker.Services;

public class GoogleSignIn: ISigInInThirdParty
{
    private const string AccessTokenKey = "thirdparty_access_token";
    private const string RefreshTokenKey = "thirdparty_refresh_token";
    private const string IdTokenKey = "thirdparty_id_token";

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

        // Persist tokens in SecureStorage for session
        if (!string.IsNullOrEmpty(accessToken))
            await SecureStorage.SetAsync(AccessTokenKey, accessToken);
        if (!string.IsNullOrEmpty(refreshToken))
            await SecureStorage.SetAsync(RefreshTokenKey, refreshToken);
        if (!string.IsNullOrEmpty(idToken))
            await SecureStorage.SetAsync(IdTokenKey, idToken);

        return new ThirdPartyAuthResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            IdToken = idToken,
            Properties = authResult.Properties
        };
    }

    public async Task SignOutAsync()
    {
        // Clear tokens from SecureStorage
        try
        {
            SecureStorage.Remove(AccessTokenKey);
            SecureStorage.Remove(RefreshTokenKey);
            SecureStorage.Remove(IdTokenKey);
        }
        catch
        {
            // ignore
        }

        // Optionally call Supabase sign-out endpoint if needed
        // For POC we just clear local tokens
        await Task.CompletedTask;
    }

    public async Task<ThirdPartyUserInfo?> GetCurrentUserAsync()
    {
        try
        {
            var idToken = await SecureStorage.GetAsync(IdTokenKey);
            if (string.IsNullOrEmpty(idToken))
                return null;

            // ID token is a JWT - decode the payload to get user info
            var parts = idToken.Split('.');
            if (parts.Length < 2)
                return null;

            string payload = parts[1];
            // Pad base64
            payload = payload.Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var bytes = Convert.FromBase64String(payload);
            var json = System.Text.Encoding.UTF8.GetString(bytes);
            var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var user = new ThirdPartyUserInfo
            {
                Id = root.TryGetProperty("sub", out var sub) ? sub.GetString() : null,
                Email = root.TryGetProperty("email", out var email) ? email.GetString() : null,
                Name = root.TryGetProperty("name", out var name) ? name.GetString() : null,
            };

            return user;
        }
        catch
        {
            return null;
        }
    }
}