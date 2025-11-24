using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace FinanceTracker.Api.Models
{
    public enum Frequency
    {
        Daily, 
        Weekly, 
        BiWeekly, 
        Monthly, 
        Quarterly, 
        SemiAnnually, 
        Annually
    }

    public class Subscription
    {
        [BsonId]
        public ObjectId Id { get; set; }

        /// <summary>
        /// Owner user id of the subscription.
        /// </summary>
        public ObjectId UserId { get; set; }

        /// <summary>
        /// Human-readable name of the subscription (e.g., "Spotify", "Netflix").
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Recurring amount (decimal). Use minor units if your app requires precision.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Currency code (optional).
        /// </summary>
        public string Currency { get; set; } = "USD";

        /// <summary>
        /// Frequency of recurrence.
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public Frequency Frequency { get; set; }

        /// <summary>
        /// Next scheduled payment (UTC).
        /// </summary>
        public DateTime NextPaymentUtc { get; set; }

        /// <summary>
        /// Whether the subscription is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Optional notes.
        /// </summary>
        public string? Notes { get; set; }
    }
}