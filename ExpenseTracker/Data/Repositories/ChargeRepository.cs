using ExpenseTracker.Models.Supabase;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Data.Repositories;

public class ChargeRepository
{
    private readonly SupabaseService _supabase;
    private readonly ILogger<ChargeRepository> _logger;

    public ChargeRepository(SupabaseService supabase, ILogger<ChargeRepository> logger)
    {
        _supabase = supabase;
        _logger = logger;
    }

    public async Task<List<ChargeDto>> ListAsync()
    {
        var result = await _supabase.Client!.From<ChargeDto>()
            .Select("*")
            .Get();
        return result.Models;
    }

    
    public async Task<List<ChargeDto>> ListByHouseholdAsync(long householdId)
    {
        var result = await _supabase.Client!.From<ChargeDto>()
            .Select("*")
            .Where(x => x.HouseholdIdFk == householdId)
            .Get();
        return result.Models;
    }

    public async Task<ChargeDto?> GetAsync(long id)
    {
        return await _supabase.Client!.From<ChargeDto>()
            .Select("*")
            .Where(x => x.Id == id)
            .Single();
    }

    public async Task<ChargeDto> SaveAsync(ChargeDto item)
    {
        if (item.Id == 0)
        {
            var result = await _supabase.Client!.From<ChargeDto>().Insert(item);
            return result.Models.First();
        }
        else
        {
            var result = await _supabase.Client!.From<ChargeDto>()
                .Where(x => x.Id == item.Id)
                .Update(item);
            return result.Models.First();
        }
    }

    public async Task DeleteAsync(long id)
    {
        await _supabase.Client!.From<ChargeDto>()
            .Where(x => x.Id == id)
            .Delete();
    }
}
