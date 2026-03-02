using System.Diagnostics;
using System.Text.Json;
using Supabase;
namespace ExpenseTracker.Data;

public static class AppSettings
{
    /// <summary>
    ///
    /// </summary>
    public static Settings Settings { get; private set; } = new Settings();
    
    /// <summary>
    ///
    /// </summary>
    public static string SupabaseUrl { get; private set; } = string.Empty;
    
    /// <summary>
    ///
    /// </summary>
    public  static string SupabaseKey { get; private set; } = string.Empty;
    
    /// <summary>
    /// 
    /// </summary>
    public static string SupabaseAnonKey { get; private set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public static string GoogleWebClientId { get; private set; } = string.Empty;
    
    /// <summary>
    ///
    /// </summary>
    public static bool IsSupabaseConfigured => !string.IsNullOrWhiteSpace(SupabaseUrl) && !string.IsNullOrWhiteSpace(SupabaseKey);
    
    /// <summary>
    ///
    /// </summary>
    public static async Task InitializeAsync()
    {
        await LoadAppSettingsAsync();
        await LoadSupabaseEnvAsync();
    }
    
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static SupabaseOptions GetSupabaseOptions()
    {
        if (!IsSupabaseConfigured)
            throw new InvalidOperationException("Supabase is not properly configured.");

        return new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = true,
        };
    }
    
    /// <summary>
    ///
    /// </summary>
    private static async Task LoadAppSettingsAsync()
    {
        try
        {
            var stream = await FileSystem.OpenAppPackageFileAsync("appsettings.json");
            var doc = await JsonSerializer.DeserializeAsync<Settings>(stream);
            if (doc is not null)
            {
                Settings = doc;
            }
        }
        catch (FileNotFoundException)
        {
            Debug.WriteLine("appsettings.json not found, using compile-time defaults.");
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    private static async Task LoadSupabaseEnvAsync()
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync("supabase.env");
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();

            foreach (var line in content.Split('\n'))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#'))
                    continue;

                var eqIndex = trimmed.IndexOf('=');
                if (eqIndex < 0)
                    continue;

                var key = trimmed[..eqIndex].Trim();
                var value = trimmed[(eqIndex + 1)..].Trim();

                switch (key)
                {
                    case "SUPABASE_URL":
                        SupabaseUrl = value;
                        break;
                    case "SUPABASE_KEY":
                        SupabaseKey = value;
                        break;
                    case "SUPABASE_ANON_KEY":
                        SupabaseAnonKey = value;
                        break;
                    case "GOOGLE_WEB_CLIENT_ID":
                        GoogleWebClientId = value;
                        break;
                }
            }
        }
        catch (FileNotFoundException)
        {
            Debug.WriteLine("supabase.env not found, Supabase integration disabled.");
        }
    }
    
    public static string FakeUserName => "Example";

    public static string FakeUserEmail => "fake_user@example.com";
}
