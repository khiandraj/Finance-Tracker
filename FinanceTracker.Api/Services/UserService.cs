using MongoDB.Driver;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Helpers;

namespace FinanceTracker.Api.Services
{
    /// <summary>
    /// Handles user-related business logic and database operations.
    /// </summary>
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;

        public UserService(IMongoClient client)
        {
            var database = client.GetDatabase("FinanceTrackerDB");
            _userCollection = database.GetCollection<User>("Users");
        }

        public ServiceResult AddUser(User user)
        {
            var existing = _userCollection.Find(u => u.Username == user.Username).FirstOrDefault();
            if (existing != null)
                return new ServiceResult(false, "Username already exists.");

            user.Password = PasswordHelper.HashPassword(user.Password);
            user.EncryptedLastLoggedOn = EncryptionHelper.Encrypt(DateTime.UtcNow.ToString("o"));

            _userCollection.InsertOne(user);
            return new ServiceResult(true, "User created successfully.", new { user.Id, user.Username, user.Role });
        }

        public ServiceResult Login(string username, string password)
        {
            var user = _userCollection.Find(u => u.Username == username).FirstOrDefault();
            if (user == null)
                return new ServiceResult(false, "User not found.", null, 404);

            bool isValid = PasswordHelper.VerifyPassword(password, user.Password);
            if (!isValid)
                return new ServiceResult(false, "Invalid password.", null, 401);

            string encryptedTime = EncryptionHelper.Encrypt(DateTime.UtcNow.ToString("o"));
            var update = Builders<User>.Update.Set(u => u.EncryptedLastLoggedOn, encryptedTime);
            _userCollection.UpdateOne(u => u.Id == user.Id, update);

            return new ServiceResult(true, $"Welcome back, {user.Username}! Last login updated securely.");
        }

        public IEnumerable<object> GetAllUsers()
        {
            var users = _userCollection.Find(_ => true).ToList();
            return users.Select(u => new
            {
                u.Username,
                LastLoggedOn = EncryptionHelper.Decrypt(u.EncryptedLastLoggedOn)
            });
        }
    }

    /// <summary>
    /// Wrapper for service responses, allowing consistent controller communication.
    /// </summary>
    public class ServiceResult
    {
        public bool Success { get; }
        public string Message { get; }
        public object? Data { get; }
        public int StatusCode { get; }

        public ServiceResult(bool success, string message, object? data = null, int statusCode = 200)
        {
            Success = success;
            Message = message;
            Data = data;
            StatusCode = statusCode;
        }
    }
}