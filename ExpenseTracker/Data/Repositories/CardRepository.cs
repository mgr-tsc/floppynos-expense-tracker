using ExpenseTracker.Models.Supabase;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Data.Repositories;

public class CardRepository
{
    private readonly SupabaseService _supabase;
    private readonly ILogger<CardRepository> _logger;

    public CardRepository(SupabaseService supabase, ILogger<CardRepository> logger)
    {
        _supabase = supabase;
        _logger = logger;
    }

    public async Task<List<CardDTO>> ListAsync()
    {
        var result = await _supabase.Client!.From<CardDTO>().Get();
        return result.Models;
    }

    public async Task<CardDTO?> GetAsync(long id)
    {
        return await _supabase.Client!.From<CardDTO>()
            .Where(x => x.Id == id)
            .Single();
    }

    public async Task<CardDTO> SaveAsync(CardDTO item)
    {
        if (item.Id == 0)
        {
            var result = await _supabase.Client!.From<CardDTO>().Insert(item);
            return result.Models.First();
        }
        else
        {
            var result = await _supabase.Client!.From<CardDTO>()
                .Where(x => x.Id == item.Id)
                .Update(item);
            return result.Models.First();
        }
    }

    public async Task DeleteAsync(long id)
    {
        await _supabase.Client!.From<CardDTO>()
            .Where(x => x.Id == id)
            .Delete();
    }
}
