using Supabase;
using System.Diagnostics;

namespace ExpenseTracker.Services;

public class SupabaseService
{
    public Client? Client { get; private set; }

    public SupabaseService()
    {
        // Lazy initialization: do not throw in constructor
        if (AppSettings.IsSupabaseConfigured)
        {
            try
            {
                var url = AppSettings.SupabaseUrl;
                var key = AppSettings.SupabaseKey;
                var options = AppSettings.GetSupabaseOptions();
                Client = new Client(url, key, options);
                // do not auto-connect here; callers control connect lifecycle
                Debug.WriteLine("SupabaseService created and client initialized.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to initialize Supabase client: {ex.Message}");
            }
        }
        else
        {
            Debug.WriteLine("SupabaseService created but AppSettings reports Supabase not configured.");
        }
    }

    public async Task InitializeAsync()
    {
        if (Client is null)
        {
            Debug.WriteLine("SupabaseService.InitializeAsync: no client configured, skipping.");
            return;
        }

        try
        {
            await Client.InitializeAsync();
            Debug.WriteLine("SupabaseService.InitializeAsync: client connection established.");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SupabaseService.InitializeAsync failed: {ex.Message}");
        }
    }
}
