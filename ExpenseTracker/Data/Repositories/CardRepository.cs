using ExpenseTracker.Models.Supabase;
using Microsoft.Extensions.Logging;
using Supabase.Postgrest;

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

    /// <summary>
    /// Get all cards for the current authenticated user
    /// </summary>
    public async Task<List<CardDto>> ListAsync()
    {
        var userId = _supabase.GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("ListAsync called with no authenticated user");
            return new List<CardDto>();
        }

        _logger.LogInformation("Loading cards for user: {UserId}", userId);

        var result = await _supabase.Client!.From<CardDto>()
            .Filter("user_id_fk", Constants.Operator.Equals, userId)
            .Get();

        _logger.LogInformation("Found {Count} cards for user", result.Models.Count);
        return result.Models;
    }

    /// <summary>
    /// Get a specific card by ID (user-filtered via RLS)
    /// </summary>
    public async Task<CardDto?> GetAsync(long id)
    {
        return await _supabase.Client!.From<CardDto>()
            .Where(x => x.Id == id)
            .Single();
    }

    /// <summary>
    /// Save a card (create or update)
    /// </summary>
    public async Task<CardDto> SaveAsync(CardDto item)
    {
        // For new cards, ensure user_id_fk is set
        if (item.Id == 0)
        {
            var userId = _supabase.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("Cannot create card: No authenticated user");
            }

            item.UserIdFk = userId;
            _logger.LogInformation("Creating new card for user: {UserId}", userId);

            var result = await _supabase.Client!.From<CardDto>().Insert(item);
            return result.Models.First();
        }
        else
        {
            _logger.LogInformation("Updating card ID: {Id}", item.Id);

            var result = await _supabase.Client!.From<CardDto>()
                .Where(x => x.Id == item.Id)
                .Update(item);
            return result.Models.First();
        }
    }

    /// <summary>
    /// Delete a card (prevents deletion if card has existing charges)
    /// </summary>
    public async Task DeleteAsync(long id)
    {
        _logger.LogInformation("Attempting to delete card ID: {Id}", id);

        // Check if card is referenced by charges
        var charges = await _supabase.Client!.From<ChargeDto>()
            .Filter("card_id_fk", Constants.Operator.Equals, id)
            .Get();

        if (charges.Models.Any())
        {
            _logger.LogWarning("Cannot delete card {Id} - has {Count} existing charges", id, charges.Models.Count);
            throw new InvalidOperationException("Cannot delete card with existing charges");
        }

        await _supabase.Client!.From<CardDto>()
            .Where(x => x.Id == id)
            .Delete();

        _logger.LogInformation("Card {Id} deleted successfully", id);
    }
}
