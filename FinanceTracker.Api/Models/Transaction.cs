using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace FinanceTracker.Api.Models
{
    public class Transaction
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public ObjectId UserId { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "USD";

        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

        public string Description { get; set; } = string.Empty;
    }
}
