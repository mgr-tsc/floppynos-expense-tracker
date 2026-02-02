using ExpenseTracker.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Data;

public class ExpenseRepository
{
    private bool _hasBeenInitialized = false;
    private readonly ILogger _logger;
    private readonly CardRepository _cardRepository;
    private readonly ProfileRepository _profileRepository;

    public ExpenseRepository(CardRepository cardRepository, ProfileRepository profileRepository,
        ILogger<ExpenseRepository> logger)
    {
        _cardRepository = cardRepository;
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
            CREATE TABLE IF NOT EXISTS Expense (
                ID INTEGER PRIMARY KEY AUTOINCREMENT,
                Date TEXT NOT NULL,
                Description TEXT NOT NULL,
                Amount TEXT NOT NULL,
                SplitPolicy TEXT NOT NULL,
                CardID INTEGER NOT NULL,
                PayerProfileID INTEGER NOT NULL
            );";
            await createTableCmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating Expense table");
            throw;
        }

        _hasBeenInitialized = true;
    }

    public async Task<List<Expense>> ListAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Expense ORDER BY Date DESC";
        var expenses = new List<Expense>();

        await using var reader = await selectCmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var expense = new Expense
            {
                ID = reader.GetInt32(0),
                Date = DateTime.Parse(reader.GetString(1)),
                Description = reader.GetString(2),
                Amount = decimal.Parse(reader.GetString(3)),
                SplitPolicy = reader.GetString(4),
                CardID = reader.GetInt32(5),
                PayerProfileID = reader.GetInt32(6)
            };

            expense.Card = await _cardRepository.GetAsync(expense.CardID);
            expense.Payer = await _profileRepository.GetAsync(expense.PayerProfileID);
            expenses.Add(expense);
        }

        return expenses;
    }

    public async Task<Expense?> GetAsync(int id)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var selectCmd = connection.CreateCommand();
        selectCmd.CommandText = "SELECT * FROM Expense WHERE ID = @id";
        selectCmd.Parameters.AddWithValue("@id", id);

        await using var reader = await selectCmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var expense = new Expense
            {
                ID = reader.GetInt32(0),
                Date = DateTime.Parse(reader.GetString(1)),
                Description = reader.GetString(2),
                Amount = decimal.Parse(reader.GetString(3)),
                SplitPolicy = reader.GetString(4),
                CardID = reader.GetInt32(5),
                PayerProfileID = reader.GetInt32(6)
            };

            expense.Card = await _cardRepository.GetAsync(expense.CardID);
            expense.Payer = await _profileRepository.GetAsync(expense.PayerProfileID);
            return expense;
        }

        return null;
    }

    public async Task<int> SaveItemAsync(Expense item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var saveCmd = connection.CreateCommand();
        if (item.ID == 0)
        {
            saveCmd.CommandText = @"
                INSERT INTO Expense (Date, Description, Amount, SplitPolicy, CardID, PayerProfileID)
                VALUES (@Date, @Description, @Amount, @SplitPolicy, @CardID, @PayerProfileID);
                SELECT last_insert_rowid();";
        }
        else
        {
            saveCmd.CommandText = @"
                UPDATE Expense
                SET Date = @Date, Description = @Description, Amount = @Amount, SplitPolicy = @SplitPolicy, CardID = @CardID, PayerProfileID = @PayerProfileID
                WHERE ID = @ID";
            saveCmd.Parameters.AddWithValue("@ID", item.ID);
        }

        saveCmd.Parameters.AddWithValue("@Date", item.Date.ToString("o"));
        saveCmd.Parameters.AddWithValue("@Description", item.Description);
        saveCmd.Parameters.AddWithValue("@Amount", item.Amount.ToString());
        saveCmd.Parameters.AddWithValue("@SplitPolicy", item.SplitPolicy);
        saveCmd.Parameters.AddWithValue("@CardID", item.CardID);
        saveCmd.Parameters.AddWithValue("@PayerProfileID", item.PayerProfileID);

        var result = await saveCmd.ExecuteScalarAsync();
        if (item.ID == 0)
        {
            item.ID = Convert.ToInt32(result);
        }

        return item.ID;
    }

    public async Task<int> DeleteItemAsync(Expense item)
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var deleteCmd = connection.CreateCommand();
        deleteCmd.CommandText = "DELETE FROM Expense WHERE ID = @ID";
        deleteCmd.Parameters.AddWithValue("@ID", item.ID);

        return await deleteCmd.ExecuteNonQueryAsync();
    }

    public async Task DropTableAsync()
    {
        await Init();
        await using var connection = new SqliteConnection(Constants.DatabasePath);
        await connection.OpenAsync();

        var dropCmd = connection.CreateCommand();
        dropCmd.CommandText = "DROP TABLE IF EXISTS Expense";
        await dropCmd.ExecuteNonQueryAsync();
        _hasBeenInitialized = false;
    }
}
