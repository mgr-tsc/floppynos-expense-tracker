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

    public async Task<List<PolicyDTO>> ListAsync()
    {
        var result = await _supabase.Client!.From<PolicyDTO>().Get();
        return result.Models;
    }

    public async Task<PolicyDTO?> GetAsync(long id)
    {
        return await _supabase.Client!.From<PolicyDTO>()
            .Where(x => x.Id == id)
            .Single();
    }
}
