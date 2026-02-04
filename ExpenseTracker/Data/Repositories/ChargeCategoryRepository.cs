using ExpenseTracker.Models.Supabase;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Data.Repositories;

public class ChargeCategoryRepository
{
    private readonly SupabaseService _supabase;
    private readonly ILogger<ChargeCategoryRepository> _logger;

    public ChargeCategoryRepository(SupabaseService supabase, ILogger<ChargeCategoryRepository> logger)
    {
        _supabase = supabase;
        _logger = logger;
    }

    public async Task<List<ChargeCategoryDTO>> ListAsync()
    {
        var result = await _supabase.Client!.From<ChargeCategoryDTO>().Get();
        return result.Models;
    }

    public async Task<ChargeCategoryDTO?> GetAsync(short id)
    {
        return await _supabase.Client!.From<ChargeCategoryDTO>()
            .Where(x => x.Id == id)
            .Single();
    }
}
