using MongoDB.Bson;

namespace FinanceTracker.Api.Models
{
    public class User
    {
        public ObjectId Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string EncryptedLastLoggedOn { get; set; } = string.Empty;
        public object? Role { get; set; }
    }
}
