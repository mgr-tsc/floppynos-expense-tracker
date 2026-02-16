using ExpenseTracker.Models.Supabase;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Data.Repositories;

public class PolicyRepository
{
    private readonly SupabaseService _supabase;
    private readonly ILogger<PolicyRepository> _logger;

    public PolicyRepository(SupabaseService supabase, ILogger<PolicyRepository> logger)
    {
        _supabase = supabase;
        _logger = logger;
    }

    public async Task<List<PolicyDto>> ListAsync()
    {
        var result = await _supabase.Client!.From<PolicyDto>().Get();
        return result.Models;
    }

    public async Task<PolicyDto?> GetAsync(long id)
    {
        return await _supabase.Client!.From<PolicyDto>()
            .Where(x => x.Id == id)
            .Single();
    }
}
