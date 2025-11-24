using System;
using System.Threading.Tasks;
using FinanceTracker.Api.Models;
using MongoDB.Bson;

namespace FinanceTracker.Api.Interfaces
{
    /// <summary>
    /// Minimal interface the SubscriptionService uses to record generated transactions.
    /// Implement this in your Transactions module or replace with your concrete type.
    /// </summary>
    public interface ITransactionService
    {
        /// <summary>
        /// Records a transaction for the given user.
        /// Return true if recorded successfully.
        /// </summary>
        Task<bool> RecordTransactionAsync(ObjectId userId, decimal amount, string currency, DateTime whenUtc, string description);
    }
}
