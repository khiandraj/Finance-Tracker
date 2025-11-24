using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace FinanceTracker.Api.Models
{
    public class BalanceRecord
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("UserId")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("Balance")]
        public decimal Balance { get; set; }

        [BsonElement("LastUpdated")]
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
