using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinanceTracker.Api.Helpers;
using FinanceTracker.Api.Interfaces;
using FinanceTracker.Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;

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
            // Basic validation using your helper
            var (isValid, error) = SubscriptionHelper.Validate(subscription.Amount, subscription.Frequency);
            if (!isValid)
                return (false, error, null);

            if (subscription.UserId == ObjectId.Empty)
                return (false, "UserId is required.", null);

            if (subscription.NextPaymentUtc == default)
                subscription.NextPaymentUtc = DateTime.UtcNow;

            await _subscriptionCollection.InsertOneAsync(subscription);
            return (true, "Subscription added.", subscription);
        }

        public async Task<IEnumerable<Subscription>> GetSubscriptionsForUserAsync(ObjectId userId, bool onlyActive = true)
        {
            var filter = Builders<Subscription>.Filter.Eq(s => s.UserId, userId);

            if (onlyActive)
            {
                filter = Builders<Subscription>.Filter.And(
                    filter,
                    Builders<Subscription>.Filter.Eq(s => s.IsActive, true));
            }

            var list = await _subscriptionCollection.Find(filter).ToListAsync();
            return list;
        }

        /// <summary>
        /// Processes subscriptions that are due at or before asOfUtc and records transactions.
        /// </summary>
        public async Task<int> ProcessDueSubscriptionsAsync(DateTime asOfUtc)
        {
            var filter = Builders<Subscription>.Filter.And(
                Builders<Subscription>.Filter.Lte(s => s.NextPaymentUtc, asOfUtc),
                Builders<Subscription>.Filter.Eq(s => s.IsActive, true));

            var dueSubscriptions = await _subscriptionCollection.Find(filter).ToListAsync();
            var processed = 0;

            foreach (var sub in dueSubscriptions)
            {
                var desc = $"Recurring payment for {sub.Name}";
                var success = await _transactionService.RecordTransactionAsync(
                    sub.UserId,
                    sub.Amount,
                    sub.Currency,
                    sub.NextPaymentUtc,
                    desc);

                if (!success)
                    continue;

                var next = SubscriptionHelper.CalculateNext(sub.NextPaymentUtc, sub.Frequency);
                var update = Builders<Subscription>.Update.Set(s => s.NextPaymentUtc, next);

                await _subscriptionCollection.UpdateOneAsync(s => s.Id == sub.Id, update);
                processed++;
            }

            return processed;
        }

        /// <summary>
        /// Soft-deletes a subscription (sets IsActive = false).
        /// </summary>
        public async Task<bool> CancelSubscriptionAsync(ObjectId subscriptionId)
        {
            var update = Builders<Subscription>.Update.Set(s => s.IsActive, false);
            var result = await _subscriptionCollection.UpdateOneAsync(
                s => s.Id == subscriptionId,
                update);

            return result.ModifiedCount > 0;
        }
    }
}
