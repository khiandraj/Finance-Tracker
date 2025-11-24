using FinanceTracker.Api.Interfaces;
using FinanceTracker.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Threading.Tasks;

namespace FinanceTracker.Api.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IMongoCollection<Transaction> _transactions;

        public TransactionService(IMongoClient client)
        {
            var db = client.GetDatabase("FinanceTrackerDB");
            _transactions = db.GetCollection<Transaction>("Transactions");
        }

        public async Task<bool> RecordTransactionAsync(ObjectId userId, decimal amount, string currency, DateTime whenUtc, string description)
        {
            var txn = new Transaction
            {
                UserId = userId,
                Amount = amount,
                Currency = currency,
                TimestampUtc = whenUtc,
                Description = description
            };

            await _transactions.InsertOneAsync(txn);
            return true;
        }
    }
}
