using System;
using System.Threading.Tasks;
using FinancialAssistant.Core.Models;
using SQLite;

namespace FinancialAssistant.Infrastructure.Data
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;

        public DatabaseService()
        {
        }

        public async Task Init()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);
            
            await _database.CreateTableAsync<Income>();
            await _database.CreateTableAsync<Expense>();
            await _database.CreateTableAsync<Category>();
            await _database.CreateTableAsync<SavingsGoal>();
            await _database.CreateTableAsync<User>();

            await TryAddColumnAsync("Users", "DisplayName", "TEXT NOT NULL DEFAULT ''");
            await TryAddColumnAsync("Users", "GoogleId", "TEXT");
            await TryAddColumnAsync("Expenses", "ReceiptId", "TEXT");
            await TryAddColumnAsync("SavingsGoals", "UserId", "INTEGER NOT NULL DEFAULT 0");

            // Seed default categories if empty
            if (await _database.Table<Category>().CountAsync() == 0)
            {
                await _database.InsertAllAsync(new[]
                {
                    new Category { Name = "Salary", Type = TransactionType.Income, Color = "#4CAF50" },
                    new Category { Name = "Food", Type = TransactionType.Expense, Color = "#F44336" },
                    new Category { Name = "Transport", Type = TransactionType.Expense, Color = "#2196F3" },
                    new Category { Name = "Housing", Type = TransactionType.Expense, Color = "#9C27B0" },
                    new Category { Name = "Entertainment", Type = TransactionType.Expense, Color = "#FF9800" },
                    new Category { Name = "Shopping", Type = TransactionType.Expense, Color = "#E91E63" }
                });
            }
        }

        private async Task TryAddColumnAsync(string table, string column, string columnDef)
        {
            try
            {
                await _database!.ExecuteAsync($"ALTER TABLE {table} ADD COLUMN {column} {columnDef}");
            }
            catch (SQLiteException)
            {
                // Column already exists — safe to ignore
            }
        }

        public SQLiteAsyncConnection Connection => _database!;
    }
}
