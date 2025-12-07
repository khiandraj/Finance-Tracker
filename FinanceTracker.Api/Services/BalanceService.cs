using MongoDB.Driver;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Helpers;

namespace FinanceTracker.Api.Services
{
    public class BalanceService
    {
        private readonly IMongoCollection<BalanceRecord> _balances;

        public BalanceService(IMongoClient client)
        {
            var database = client.GetDatabase("FinanceTrackerDB");
            _balances = database.GetCollection<BalanceRecord>("UserBalances");
        }

        public async Task<BalanceRecord?> GetBalanceByUserId(string userId)
        {
            return await _balances.Find(b => b.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<BalanceRecord> CreateBalanceRecord(string userId)
        {
            var newBalance = new BalanceRecord
            {
                UserId = userId,
                Balance = 0
            };

            await _balances.InsertOneAsync(newBalance);
            return newBalance;
        }

        public async Task<BalanceRecord> Credit(string userId, decimal amount)
        {
            var record = await GetBalanceByUserId(userId)
                ?? await CreateBalanceRecord(userId);

            record.Balance = BalanceHelper.ApplyCredit(record.Balance, amount);
            record.LastUpdated = DateTime.UtcNow;

            await _balances.ReplaceOneAsync(b => b.Id == record.Id, record);
            return record;
        }

        public async Task<BalanceRecord> Debit(string userId, decimal amount)
        {
            var record = await GetBalanceByUserId(userId)
                ?? await CreateBalanceRecord(userId);

            record.Balance = BalanceHelper.ApplyDebit(record.Balance, amount);
            record.LastUpdated = DateTime.UtcNow;

            await _balances.ReplaceOneAsync(b => b.Id == record.Id, record);
            return record;
        }

        public async Task<bool> DeleteBalanceRecord(string userId)
        {
            var result = await _balances.DeleteOneAsync(b => b.UserId == userId);
            return result.DeletedCount > 0;
        }
    }
}
