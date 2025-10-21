using CryptoOculus.Models;
using MySql.Data.MySqlClient;
using System.Data.Common;

namespace CryptoOculus.Services
{
    public class UserService(IConfiguration configuration)
    {
        public async Task<bool> AddUser(long telegramId, string languageCode)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "INSERT INTO users (telegramId, languageCode, lastActionDate) VALUES (@telegramId, @languageCode, @lastActionDate)";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@languageCode", languageCode);
            command.Parameters.AddWithValue("@lastActionDate", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteUser(long telegramId)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "DELETE FROM users WHERE telegramId = @telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DoesUserExist(long telegramId)
        {
            await using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "SELECT id FROM users WHERE telegramId = @telegramId LIMIT 1";

            await using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);

            await using DbDataReader reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync();
        }

        public async Task<UserSettings> GetUserSettings(long telegramId)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "SELECT languageCode, profit, amount, lastActionDate, newSpreadsDelay, buyExBlacklist, sellExBlacklist, spreadsBlacklist, coinsBlacklist, subscribedItem FROM users WHERE telegramId = @telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? new()
            {
                TelegramId = telegramId,
                LanguageCode = reader.GetString(0),
                Profit = reader.GetDouble(1),
                Amount = reader.GetDouble(2),
                LastActionDate = reader.GetInt64(3),
                NewSpreadsDelay = reader.GetInt64(4),
                BuyExBlacklist = reader.GetString(5),
                SellExBlacklist = reader.GetString(6),
                SpreadsBlacklist = reader.GetString(7),
                CoinsBlacklist = reader.GetString(8),
                SubscribedItem = !reader.IsDBNull(9) ? reader.GetString(9) : null
            } : throw new Exception($"UserSettings datas not found for telegram id: {telegramId}");
        }
        public async Task<UserSettings[]> GetUsersSettings()
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "SELECT telegramId, languageCode, profit, amount, lastActionDate, setUp, newSpreadsDelay, buyExBlacklist, sellExBlacklist, spreadsBlacklist, coinsBlacklist, subscribedItem FROM users";

            using MySqlCommand command = new(query, connection);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            List<UserSettings> userSettings = [];

            while (await reader.ReadAsync())
            {
                if (reader.GetBoolean(5))
                {
                    userSettings.Add(new()
                    {
                        TelegramId = reader.GetInt64(0),
                        LanguageCode = reader.GetString(1),
                        Profit = reader.GetDouble(2),
                        Amount = reader.GetDouble(3),
                        LastActionDate = reader.GetInt64(4),
                        NewSpreadsDelay = reader.GetInt64(6),
                        BuyExBlacklist = reader.GetString(7),
                        SellExBlacklist = reader.GetString(8),
                        SpreadsBlacklist = reader.GetString(9),
                        CoinsBlacklist = reader.GetString(10),
                        SubscribedItem = !reader.IsDBNull(11) ? reader.GetString(11) : null
                    });
                }
            }

            return [.. userSettings];
        }
        

        public async Task<string> GetUserLanguageCode(long telegramId)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "SELECT languageCode FROM users WHERE telegramId = @telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? reader.GetString(0) : throw new Exception($"languageCode not found for telegram id: {telegramId}");
        }

        public async Task<string?> GetUserWaitingMethods(long telegramId)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "SELECT waitingMethods FROM users WHERE telegramId = @telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() && !reader.IsDBNull(0) ? reader.GetString(0) : null;
        }

        public async Task<long> GetUserLastActionDate(long telegramId)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "SELECT lastActionDate FROM users WHERE telegramId = @telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? reader.GetInt64(0) : throw new Exception($"lastActionDate not found for telegram id: {telegramId}");
        }

        public async Task<bool> GetUserSetUp(long telegramId)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "SELECT setUp FROM users WHERE telegramId = @telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() && reader.GetInt16(0) == 1;
        }

        public async Task<long> GetUserNewSpreadsDelay(long telegramId)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "SELECT newSpreadsDelay FROM users WHERE telegramId = @telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? reader.GetInt64(0) : throw new Exception($"newSpreadsDelay not found for telegram id: {telegramId}");
        }

        public async Task<string> GetUserBuyExBlacklist(long telegramId)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "SELECT buyExBlacklist FROM users WHERE telegramId = @telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? reader.GetString(0) : throw new Exception($"buyExBlacklist not found for telegram id: {telegramId}");
        }

        public async Task<string> GetUserSellExBlacklist(long telegramId)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "SELECT sellExBlacklist FROM users WHERE telegramId = @telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() ? reader.GetString(0) : throw new Exception($"sellExBlacklist not found for telegram id: {telegramId}");
        }

        public async Task<string?> GetUserSubscribedItem(long telegramId)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "SELECT subscribedItem FROM users WHERE telegramId = @telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() && !reader.IsDBNull(0) ? reader.GetString(0) : null;
        }

        public async Task<string?> GetUserSubscribedItemLastMessage(long telegramId)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "SELECT subscribedItemLastMessage FROM users WHERE telegramId = @telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);

            using DbDataReader reader = await command.ExecuteReaderAsync();

            return await reader.ReadAsync() && !reader.IsDBNull(0) ? reader.GetString(0) : null;
        }


        public async Task<bool> UpdateUserLanguageCode(long telegramId, string languageCode)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "UPDATE users SET languageCode=@languageCode WHERE telegramId=@telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@languageCode", languageCode);
            command.Parameters.AddWithValue("@telegramId", telegramId.ToString());

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateUserProfit(long telegramId, double profit)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "UPDATE users SET profit=@profit WHERE telegramId=@telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@profit", profit);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateUserAmount(long telegramId, double amount)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "UPDATE users SET amount=@amount WHERE telegramId=@telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@amount", amount);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateUserWaitingMethods(long telegramId, string? json)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "UPDATE users SET waitingMethods=@waitingMethods WHERE telegramId=@telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@waitingMethods", json);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateUserLastActionDate(long telegramId, long timestamp)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "UPDATE users SET lastActionDate=@lastActionDate WHERE telegramId=@telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@lastActionDate", timestamp);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateUserSetUp(long telegramId, bool status)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "UPDATE users SET setUp=@setUp WHERE telegramId=@telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@setUp", status ? 1 : 0);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateUserNewSpreadsDelay(long telegramId, long seconds)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "UPDATE users SET newSpreadsDelay=@newSpreadsDelay WHERE telegramId=@telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@newSpreadsDelay", seconds);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateUserBuyExBlacklist(long telegramId, string buyExBlacklist)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "UPDATE users SET buyExBlacklist=@buyExBlacklist WHERE telegramId=@telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@buyExBlacklist", buyExBlacklist);
            command.Parameters.AddWithValue("@telegramId", telegramId.ToString());

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateUserSellExBlacklist(long telegramId, string sellExBlacklist)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "UPDATE users SET sellExBlacklist=@sellExBlacklist WHERE telegramId=@telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@sellExBlacklist", sellExBlacklist);
            command.Parameters.AddWithValue("@telegramId", telegramId.ToString());

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateUserSubscribedItem(long telegramId, string? json)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "UPDATE users SET subscribedItem=@subscribedItem WHERE telegramId=@telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@subscribedItem", json);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateUserSubscribedItemLastMessage(long telegramId, string? json)
        {
            using MySqlConnection connection = new(configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string query = "UPDATE users SET subscribedItemLastMessage=@subscribedItemLastMessage WHERE telegramId=@telegramId";

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@telegramId", telegramId);
            command.Parameters.AddWithValue("@subscribedItemLastMessage", json);

            return await command.ExecuteNonQueryAsync() > 0;
        }
    }
}