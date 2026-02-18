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

    public async Task<List<ChargeCategoryDto>> ListAsync()
    {
        var result = await _supabase.Client!.From<ChargeCategoryDto>().Get();
        return result.Models;
    }

    public async Task<ChargeCategoryDto?> GetAsync(short id)
    {
        return await _supabase.Client!.From<ChargeCategoryDto>()
            .Where(x => x.Id == id)
            .Single();
    }

    public async Task<ChargeCategoryDto> SaveAsync(ChargeCategoryDto item)
    {
        var result = await _supabase.Client!.From<ChargeCategoryDto>().Insert(item);
        return result.Models.First();
    }
}
