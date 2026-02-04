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

    public async Task<List<ChargeDTO>> ListAsync()
    {
        var result = await _supabase.Client!.From<ChargeDTO>()
            .Select("*, POLICY(*), CHARGE_CAT_UNUSED(*), CARD_RECORD(*)")
            .Get();
        return result.Models;
    }

    public async Task<ChargeDTO?> GetAsync(long id)
    {
        return await _supabase.Client!.From<ChargeDTO>()
            .Select("*, POLICY(*), CHARGE_CAT_UNUSED(*), CARD_RECORD(*)")
            .Where(x => x.Id == id)
            .Single();
    }

    public async Task<ChargeDTO> SaveAsync(ChargeDTO item)
    {
        if (item.Id == 0)
        {
            var result = await _supabase.Client!.From<ChargeDTO>().Insert(item);
            return result.Models.First();
        }
        else
        {
            var result = await _supabase.Client!.From<ChargeDTO>()
                .Where(x => x.Id == item.Id)
                .Update(item);
            return result.Models.First();
        }
    }

    public async Task DeleteAsync(long id)
    {
        await _supabase.Client!.From<ChargeDTO>()
            .Where(x => x.Id == id)
            .Delete();
    }
}
