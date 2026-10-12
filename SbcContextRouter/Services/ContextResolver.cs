using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace SbcContextRouter.Services
{
    public class ContextResolver
    {
        private readonly string _connectionString;

        public ContextResolver()
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "routing_cache.db");
            _connectionString = $"Data Source={dbPath};Cache=Shared";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS ContextCache (
                PhoneNumber TEXT PRIMARY KEY,
                AccountId TEXT NOT NULL,
                RouteProfile TEXT NOT NULL,
                Version INTEGER NOT NULL
            );
            INSERT OR IGNORE INTO ContextCache (PhoneNumber, AccountId, RouteProfile, Version) 
            VALUES ('+17175550199', 'ACC-90210-VIP', 'PRIORITY_QUEUE', 1);";
            command.ExecuteNonQuery();
        }

        public async Task<(string AccountId, string Profile, bool IsFallback, int Version)> ResolveContextAsync(string phoneNumber, int latencyBudgetMs = 5)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(latencyBudgetMs));
            try
            {
                return await Task.Run(async () =>
                {
                    using var connection = new SqliteConnection(_connectionString);
                    await connection.OpenAsync(cts.Token);

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT AccountId, RouteProfile, Version FROM ContextCache WHERE PhoneNumber = $phone;";
                    command.Parameters.AddWithValue("$phone", phoneNumber);

                    using var reader = await command.ExecuteReaderAsync(cts.Token);
                    if (await reader.ReadAsync(cts.Token))
                    {
                        return (
                            reader.GetString(0),
                            reader.GetString(1),
                            false,
                            reader.GetInt32(2)
                        );
                    }

                    return ("ACC-DEFAULT-STANDARD", "STANDARD_QUEUE", false, 1);
                }, cts.Token);
            }
            catch (OperationCanceledException)
            {
                // Latency budget expired -> Deterministic Fallback Context triggered
                return ("ACC-DEFAULT-FALLBACK", "FALLBACK_QUEUE", true, 1);
            }
        }
    }
}
