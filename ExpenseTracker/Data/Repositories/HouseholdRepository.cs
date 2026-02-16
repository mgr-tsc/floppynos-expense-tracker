using ExpenseTracker.Models.Supabase;
using Microsoft.Extensions.Logging;
using Supabase.Postgrest;

namespace ExpenseTracker.Data.Repositories;

public class HouseholdRepository
{
    private readonly SupabaseService _supabase;
    private readonly ILogger<HouseholdRepository> _logger;

    public HouseholdRepository(SupabaseService supabase, ILogger<HouseholdRepository> logger)
    {
        _supabase = supabase;
        _logger = logger;
    }

    public async Task<List<HouseHoldDto>> ListAsync()
    {
        var result = await _supabase.Client!.From<HouseHoldDto>().Get();
        return result.Models;
    }

    public async Task<HouseHoldDto?> GetAsync(long id)
    {
        return await _supabase.Client!.From<HouseHoldDto>()
            .Where(x => x.Id == id)
            .Single();
    }

    public async Task<HouseHoldDto> SaveAsync(HouseHoldDto item)
    {
        if (item.Id == 0)
        {
            var result = await _supabase.Client!.From<HouseHoldDto>().Insert(item);
            return result.Models.First();
        }
        else
        {
            var result = await _supabase.Client!.From<HouseHoldDto>()
                .Where(x => x.Id == item.Id)
                .Update(item);
            return result.Models.First();
        }
    }

    public async Task DeleteAsync(long id)
    {
        await _supabase.Client!.From<HouseHoldDto>()
            .Where(x => x.Id == id)
            .Delete();
    }

    /// <summary>
    /// Check if a user belongs to any household (as user_a or user_b)
    /// </summary>
    public async Task<HouseHoldDto?> GetByUserIdAsync(string userId)
    {
        _logger.LogInformation("[HouseholdRepository] Searching for household with userId: {UserId}", userId);

        // Query user_a_id_fk = userId
        var result = await _supabase.Client!.From<HouseHoldDto>()
            .Filter("user_a_id_fk", Constants.Operator.Equals, userId)
            .Get();

        _logger.LogInformation("[HouseholdRepository] Found {Count} households where user is user_a", result.Models.Count);

        if (result.Models.Any())
        {
            var household = result.Models.First();
            _logger.LogInformation("[HouseholdRepository] Returning household ID: {Id}", household.Id);
            return household;
        }

        // Query user_b_id_fk = userId
        result = await _supabase.Client!.From<HouseHoldDto>()
            .Filter("user_b_id_fk", Constants.Operator.Equals, userId)
            .Get();

        _logger.LogInformation("[HouseholdRepository] Found {Count} households where user is user_b", result.Models.Count);

        return result.Models.FirstOrDefault();
    }

    /// <summary>
    /// Find a household by its join code
    /// </summary>
    public async Task<HouseHoldDto?> GetByCodeAsync(long code)
    {
        var result = await _supabase.Client!.From<HouseHoldDto>()
            .Filter("code", Constants.Operator.Equals, code)
            .Filter("enabled", Constants.Operator.Equals, true)
            .Get();
        return result.Models.FirstOrDefault();
    }
}
