using System.Text.Json;
using ExpenseTracker.Models;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Data;

public class SeedDataService
{
    private readonly ProfileRepository _profileRepository;
    private readonly CardRepository _cardRepository;
    private readonly ExpenseRepository _expenseRepository;
    private readonly string _seedDataFilePath = "SeedData.json";
    private readonly ILogger<SeedDataService> _logger;

    public SeedDataService(ProfileRepository profileRepository, CardRepository cardRepository,
        ExpenseRepository expenseRepository, ILogger<SeedDataService> logger)
    {
        _profileRepository = profileRepository;
        _cardRepository = cardRepository;
        _expenseRepository = expenseRepository;
        _logger = logger;
    }

    public async Task LoadSeedDataAsync()
    {
        ClearTables();

        await using Stream templateStream = await FileSystem.OpenAppPackageFileAsync(_seedDataFilePath);

        SeedData? payload = null;
        try
        {
            payload = JsonSerializer.Deserialize(templateStream, JsonContext.Default.SeedData);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error deserializing seed data");
        }

        try
        {
            if (payload is not null)
            {
                var savedProfiles = new List<Profile>();
                foreach (var profile in payload.Profiles)
                {
                    await _profileRepository.SaveItemAsync(profile);
                    savedProfiles.Add(profile);
                }

                var savedCards = new List<Card>();
                foreach (var seedCard in payload.Cards)
                {
                    var card = new Card
                    {
                        Provider = seedCard.Provider,
                        Alias = seedCard.Alias,
                        LastFourDigits = seedCard.LastFourDigits,
                        ProfileID = savedProfiles[seedCard.ProfileIndex].ID
                    };
                    await _cardRepository.SaveItemAsync(card);
                    savedCards.Add(card);
                }

                foreach (var seedExpense in payload.Expenses)
                {
                    var card = savedCards[seedExpense.CardIndex];
                    var expense = new Expense
                    {
                        Description = seedExpense.Description,
                        Amount = seedExpense.Amount,
                        SplitPolicy = seedExpense.SplitPolicy,
                        CardID = card.ID,
                        PayerProfileID = card.ProfileID,
                        Date = DateTime.Today.AddDays(-seedExpense.DaysAgo)
                    };
                    await _expenseRepository.SaveItemAsync(expense);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving seed data");
            throw;
        }
    }

    private async void ClearTables()
    {
        try
        {
            await Task.WhenAll(
                _expenseRepository.DropTableAsync(),
                _cardRepository.DropTableAsync(),
                _profileRepository.DropTableAsync());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}

public class SeedData
{
    public List<Profile> Profiles { get; set; } = [];
    public List<SeedCard> Cards { get; set; } = [];
    public List<SeedExpense> Expenses { get; set; } = [];
}

public class SeedCard
{
    public string Provider { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public string LastFourDigits { get; set; } = string.Empty;
    public int ProfileIndex { get; set; }
    public int ProfileID { get; set; }
    public int ID { get; set; }
}

public class SeedExpense
{
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string SplitPolicy { get; set; } = "50/50";
    public int CardIndex { get; set; }
    public int DaysAgo { get; set; }
    public int CardID { get; set; }
    public int PayerProfileID { get; set; }
    public DateTime Date { get; set; }
    public int ID { get; set; }
}
