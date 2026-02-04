using ExpenseTracker.Models.Supabase;
using Microsoft.Extensions.Logging;

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

    public async Task<List<HouseHoldDTO>> ListAsync()
    {
        var result = await _supabase.Client!.From<HouseHoldDTO>().Get();
        return result.Models;
    }

    public async Task<HouseHoldDTO?> GetAsync(long id)
    {
        return await _supabase.Client!.From<HouseHoldDTO>()
            .Where(x => x.Id == id)
            .Single();
    }

    public async Task<HouseHoldDTO> SaveAsync(HouseHoldDTO item)
    {
        if (item.Id == 0)
        {
            var result = await _supabase.Client!.From<HouseHoldDTO>().Insert(item);
            return result.Models.First();
        }
        else
        {
            var result = await _supabase.Client!.From<HouseHoldDTO>()
                .Where(x => x.Id == item.Id)
                .Update(item);
            return result.Models.First();
        }
    }

    public async Task DeleteAsync(long id)
    {
        await _supabase.Client!.From<HouseHoldDTO>()
            .Where(x => x.Id == id)
            .Delete();
    }
}
