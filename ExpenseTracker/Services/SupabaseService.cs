using Supabase;
using System.Diagnostics;
using System.Text.Json;
using ExpenseTracker.Models.Supabase;
using ExpenseTracker.Services.Interfaces;

namespace ExpenseTracker.Services;

public class SupabaseService
{
    public Client? Client { get; private set; }
    private JsonSerializerOptions? JsonSerializerOptions => new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private ThirdPartyUserInfo GetFakeUser => new ThirdPartyUserInfo
    {
        Name = AppSettings.FakeUserName,
        Email = AppSettings.FakeUserEmail,
        Id = "fake_user_id"
    };

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

    /// <summary>
    /// Gets the current authenticated user's ID
    /// </summary>
    public string? GetCurrentUserId() => Client?.Auth.CurrentUser?.Id;

    public async Task<Tuple<string, string>> CreateHouseHoldTrackerAsync(string houseHoldName)
    {
        try
        {
            var code = new Random().Next(10000, 99999);
            var userEmail = Client?.Auth.CurrentUser?.Email ?? string.Empty;
            var tempRecord = CreateHouseHoldTemporaryRecord(houseHoldName, code, userEmail);
            var userId = Client.Auth.CurrentUser!.Id;

            var parameters = new Dictionary<string, object>
            {
                { "p_household_name", houseHoldName },
                { "p_user_a_id", userId },
                { "p_code", (long)code },
                { "p_temp_key", tempRecord.Key },
                { "p_temp_record", tempRecord.Record },
                { "p_expire_at", tempRecord.ExpireAt.ToString("O") }
            };

            Debug.WriteLine($"Calling create_household_with_temp_record:");
            Debug.WriteLine($"  p_household_name: {houseHoldName}");
            Debug.WriteLine($"  p_user_a_id: {userId}");
            Debug.WriteLine($"  p_code: {code}");
            Debug.WriteLine($"  p_temp_key: {tempRecord.Key}");
            Debug.WriteLine($"  p_expire_at: {tempRecord.ExpireAt:O}");

            var storeProcedureCallResult = await Client!
                .Rpc("create_household_with_temp_record", parameters);
            return new Tuple<string, string>(storeProcedureCallResult.Content.ToString(), code.ToString());
        }
        catch (Exception e)
        {
            Debug.WriteLine($"SupabaseService.CreateHouseHoldTrackerAsync failed: {e.Message}");
            throw;
        }
    }

    public async Task<BalanceData?> GetHouseholdBalanceAsync(long householdId)
    {
        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "p_household_id", householdId }
            };

            var result = await Client!.Rpc("calculate_household_balance", parameters);
            var content = result.Content?.ToString();

            if (string.IsNullOrEmpty(content))
                return null;

            if (content.TrimStart().StartsWith('['))
            {
                var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<BalanceData>>(content);
                return list?.FirstOrDefault();
            }

            return Newtonsoft.Json.JsonConvert.DeserializeObject<BalanceData>(content);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SupabaseService.GetHouseholdBalanceAsync failed: {ex.Message}");
            return null;
        }
    }

    private TemporaryRecordDto CreateHouseHoldTemporaryRecord(string houseHoldName, int code, string userEmail)
    {
        var houseHoldCreationData = new HouseHoldCreationRecord
        {
            HouseHoldName = houseHoldName,
            CreatedAt = DateTimeOffset.UtcNow,
            UserCreationEmail = userEmail,
            CodeToJoin = code.ToString(),
            Uuid = Guid.NewGuid().ToString()
        };
        var tempRecord = new TemporaryRecordDto
        {
            CreatedAt = DateTimeOffset.UtcNow,
            ExpireAt = DateTimeOffset.UtcNow.AddMinutes(60),
            Key = $"_household_creation_{houseHoldName}",
            Record = JsonSerializer.Serialize(houseHoldCreationData, JsonSerializerOptions)
        };
        return tempRecord;
    }
}
