using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Helpers;
using FinanceTracker.Api.Interfaces;
using MongoDB.Bson;

namespace FinanceTracker.Api.Services
{
    public class SubscriptionService
    {
        private readonly IMongoCollection<Subscription> _subscriptionCollection;
        private readonly ITransactionService _transactionService;

        public SubscriptionService(IMongoClient client, ITransactionService transactionService)
        {
            var db = client.GetDatabase("FinanceTrackerDB");
            _subscriptionCollection = db.GetCollection<Subscription>("Subscriptions");
            _transactionService = transactionService;
        }

        public async Task<(bool Success, string? Message, Subscription? Data)> AddSubscriptionAsync(Subscription subscription)
        {
            var (isValid, error) = SubscriptionHelper.Validate(subscription.Amount, subscription.Frequency);
            if (!isValid)
                return (false, error, null);

            if (subscription.NextPaymentUtc == default)
                subscription.NextPaymentUtc = DateTime.UtcNow;

            await _subscriptionCollection.InsertOneAsync(subscription);
            return (true, "Subscription added.", subscription);
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsForUserAsync(ObjectId userId, bool onlyActive = true)
        {
            var filter = Builders<Subscription>.Filter.Eq(s => s.UserId, userId);
            if (onlyActive)
                filter = Builders<Subscription>.Filter.And(filter, Builders<Subscription>.Filter.Eq(s => s.IsActive, true));

            var list = await _subscriptionCollection.Find(filter).ToListAsync();
            return list;
        }

        /// <summary>
        /// Processes all subscriptions that are due at or before 'asOfUtc' - creates transactions and updates NextPaymentUtc.
        /// Returns the count of processed subscriptions.
        /// </summary>
        public async Task<int> ProcessDueSubscriptionsAsync(DateTime asOfUtc)
        {
            var filter = Builders<Subscription>.Filter.And(
                Builders<Subscription>.Filter.Lte(s => s.NextPaymentUtc, asOfUtc),
                Builders<Subscription>.Filter.Eq(s => s.IsActive, true)
            );

            var dueSubscriptions = await _subscriptionCollection.Find(filter).ToListAsync();
            var processed = 0;

            foreach (var sub in dueSubscriptions)
            {
                // Create a transaction record via the ITransactionService
                var desc = $"Recurring payment for {sub.Name}";
                var success = await _transactionService.RecordTransactionAsync(sub.UserId, sub.Amount, sub.Currency, sub.NextPaymentUtc, desc);

                if (!success)
                {
                    // Could optionally log or mark failure; skip updating next date for failed transactions
                    continue;
                }

                // Update NextPaymentUtc to the next occurrence
                var next = SubscriptionHelper.CalculateNext(sub.NextPaymentUtc, sub.Frequency);
                var update = Builders<Subscription>.Update.Set(s => s.NextPaymentUtc, next);
                await _subscriptionCollection.UpdateOneAsync(s => s.Id == sub.Id, update);
                processed++;
            }

            return processed;
        }

        /// <summary>
        /// Cancel (soft delete) subscription.
        /// </summary>
        public async Task<bool> CancelSubscriptionAsync(ObjectId subscriptionId)
        {
            var update = Builders<Subscription>.Update.Set(s => s.IsActive, false);
            var result = await _subscriptionCollection.UpdateOneAsync(s => s.Id == subscriptionId, update);
            return result.ModifiedCount > 0;
        }
    }
}
