using ExpenseTracker.Models.Supabase;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Data.Repositories;

public class PaymentRepository
{
    private readonly SupabaseService _supabase;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(SupabaseService supabase, ILogger<PaymentRepository> logger)
    {
        _supabase = supabase;
        _logger = logger;
        _logger.LogDebug("PaymentRepository created");
    }

    public async Task<List<PaymentDto>> ListAsync()
    {
        // Include the related Charge record
        var result = await _supabase.Client!.From<PaymentDto>()
            .Select("*, CHARGE(*)")
            .Get();
        return result.Models;
    }

    public async Task<PaymentDto?> GetAsync(long id)
    {
        return await _supabase.Client!.From<PaymentDto>()
            .Select("*, CHARGE(*)")
            .Where(x => x.Id == id)
            .Single();
    }

    public async Task<PaymentDto> SaveAsync(PaymentDto item)
    {
        if (item.Id == 0)
        {
            var result = await _supabase.Client!.From<PaymentDto>().Insert(item);
            return result.Models.First();
        }
        else
        {
            var result = await _supabase.Client!.From<PaymentDto>()
                .Where(x => x.Id == item.Id)
                .Update(item);
            return result.Models.First();
        }
    }

    public async Task DeleteAsync(long id)
    {
        await _supabase.Client!.From<PaymentDto>()
            .Where(x => x.Id == id)
            .Delete();
    }
}