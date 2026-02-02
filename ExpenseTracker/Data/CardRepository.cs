using ExpenseTracker.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Data;

public class CardRepository
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;
    private readonly ProfileRepository _profileRepository;

    public CardRepository(ProfileRepository profileRepository, ILogger<CardRepository> logger)
    {
        _profileRepository = profileRepository;
        _logger = logger;
    }

    private async Task Init()
    {
        if (_hasBeenInitialized)
            return;

        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        try
        {
            var createTableCmd = connection.CreateCommand();
            createTableCmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS Card (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Provider TEXT NOT NULL,
                Alias TEXT NOT NULL,
                LastFourDigits TEXT NOT NULL,
                ProfileID INTEGER NOT NULL
            );";
            await createTableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating Card table");
            throw;
        }

        _hasBeenInitialized = true;
    }

    public async Task<List<Card>> ListAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Card";
        var cards = new List<Card>();

        await using var reader = await selectCmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var card = new Card
            {
                ID = reader.GetInt32(0),
                Provider = reader.GetString(1),
                Alias = reader.GetString(2),
                LastFourDigits = reader.GetString(3),
                ProfileID = reader.GetInt32(4)
            };

            card.Profile = await _profileRepository.GetAsync(card.ProfileID);
            cards.Add(card);
        }

        return cards;
    }

    public async Task<List<Card>> ListAsync(int profileId)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Card WHERE ProfileID = @profileId";
        selectCmd.Parameters.AddWithValue("@profileId", profileId);
        var cards = new List<Card>();

        await using var reader = await selectCmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var card = new Card
            {
                ID = reader.GetInt32(0),
                Provider = reader.GetString(1),
                Alias = reader.GetString(2),
                LastFourDigits = reader.GetString(3),
                ProfileID = reader.GetInt32(4)
            };

            card.Profile = await _profileRepository.GetAsync(card.ProfileID);
            cards.Add(card);
        }

        return cards;
    }

    public async Task<Card?> GetAsync(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Card WHERE ID = @id";
        selectCmd.Parameters.AddWithValue("@id", id);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var card = new Card
            {
                ID = reader.GetInt32(0),
                Provider = reader.GetString(1),
                Alias = reader.GetString(2),
                LastFourDigits = reader.GetString(3),
                ProfileID = reader.GetInt32(4)
            };

            card.Profile = await _profileRepository.GetAsync(card.ProfileID);
            return card;
        }

        return null;
    }

    public async Task<int> SaveItemAsync(Card item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var saveCmd = connection.CreateCommand();
        if (item.ID == 0)
        {
            saveCmd.CommandText = @"
                INSERT INTO Card (Provider, Alias, LastFourDigits, ProfileID)
                VALUES (@Provider, @Alias, @LastFourDigits, @ProfileID);
                SELECT last_insert_rowid();";
        }
        else
        {
            saveCmd.CommandText = @"
                UPDATE Card
                SET Provider = @Provider, Alias = @Alias, LastFourDigits = @LastFourDigits, ProfileID = @ProfileID
                WHERE ID = @ID";
            saveCmd.Parameters.AddWithValue("@ID", item.ID);
        }

        saveCmd.Parameters.AddWithValue("@Provider", item.Provider);
        saveCmd.Parameters.AddWithValue("@Alias", item.Alias);
        saveCmd.Parameters.AddWithValue("@LastFourDigits", item.LastFourDigits);
        saveCmd.Parameters.AddWithValue("@ProfileID", item.ProfileID);

        var result = await saveCmd.ExecuteScalarAsync();
        if (item.ID == 0)
        {
            item.ID = Convert.ToInt32(result);
        }

        return item.ID;
    }

    public async Task<int> DeleteItemAsync(Card item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Card WHERE ID = @ID";
        deleteCmd.Parameters.AddWithValue("@ID", item.ID);

        return await deleteCmd.ExecuteNonQueryAsync();
    }

    public async Task DropTableAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var dropCmd = connection.CreateCommand();
        dropCmd.CommandText = "DROP TABLE IF EXISTS Card";
        await dropCmd.ExecuteNonQueryAsync();
        _hasBeenInitialized = false;
    }
}
