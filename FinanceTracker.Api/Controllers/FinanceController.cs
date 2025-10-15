using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Cryptography;
using System.Text;


namespace FinanceTracker.Api.Controllers
{
    /// <summary>
    /// Controller responsible for managing user registration, authentication,
    /// and retrieval of registered users in the Finance Tracker API.
    /// </summary>

    [ApiController]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {

        /// <summary>
        /// In-memory storage for all registered users.
        /// </summary>
        private readonly IMongoCollection<User> _userCollection;

        public FinanceController()
        {

            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("FinanceTrackerDB");
            _userCollection = database.GetCollection<User>("Users");
        }


        [HttpPost("adduser")]
        public ActionResult<User> AddUser([FromBody] User user)
        {
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                return BadRequest("Username and password are required.");
            if (_users.Any(u => u.Username == user.Username))
                return Conflict("Username already exists.");

            user.Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1;

            user.Password = PasswordHelper.HashPassword(user.Password);
            _users.Add(user);

            return Ok(new { user.Id, user.Username });
        }

        /// <summary>
        /// Retrieves a list of all registered users.
        /// </summary>
        /// <returns>
        /// A collection of <see cref="User"/> objects currently stored in memory.
        /// </returns>
        /// <example>
        /// GET: api/finance/users  
        /// Response:
        /// <code>
        /// [
        ///   { "id": 1, "username": "alex123", "password": "******" },
        ///   { "id": 2, "username": "jane_doe", "password": "******" }
        /// ]
        /// </code>
        /// </example>

        [HttpGet("users")]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            return Ok(_users);
        }

        /// <summary>
        /// Authenticates a user based on username and password.
        /// </summary>
        /// <param name="loginRequest">An object containing the username and password to verify.</param>
        /// <returns>
        /// A welcome message if login is successful.
        /// Returns:
        /// - 400 Bad Request if username or password is missing.
        /// - 404 Not Found if the username does not exist.
        /// - 401 Unauthorized if the password is incorrect.
        /// - 200 OK with a success message if credentials are valid.
        /// </returns>
        /// <example>
        /// POST: api/finance/login  
        /// Request Body:
        /// <code>
        /// {
        ///   "username": "alex123",
        ///   "password": "SecurePass123"
        /// }
        /// </code>
        /// Response:
        /// <code>
        /// "Welcome back, alex123!"
        /// </code>
        /// </example>

        
        [HttpPost("login")]
        public ActionResult<string> Login([FromBody] User loginRequest)
            {
            if (string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
                return BadRequest("Username and password are required.");

            var user = _userCollection.Find(u => u.Username == loginRequest.Username).FirstOrDefault();
            if (user == null)
                return NotFound("User not found.");

            bool isValid = PasswordHelper.VerifyPassword(loginRequest.Password, user.Password);
            if (!isValid)
                return Unauthorized("Invalid password.");

            
            string encryptedTime = EncryptionHelper.Encrypt(DateTime.UtcNow.ToString("o"));
            var update = Builders<User>.Update.Set(u => u.EncryptedLastLoggedOn, encryptedTime);
            _userCollection.UpdateOne(u => u.Id == user.Id, update);

            return Ok($"Welcome back, {user.Username}! Last login updated securely.");
        }

    }



    public class User
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// Username used to log in.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User's password (should be hashed and secured in production).
        /// </summary>
        public string Password { get; set; } = string.Empty;
        public string EncryptedLastLoggedOn { get; set; } = string.Empty;
    }

    public static class PasswordHelper
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }

    public static class EncryptionHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456");

        public static string Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            var encryptor = aes.CreateEncryptor(aes.Key, aes, IV);
            var bytes = Encoding.UTF8.GetBytes(plainText);

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(bytes, 0, bytes.Length);
                cs.FlushFinalBlock();
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var buffer = Convert.FromBase64String(cipherText);

            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            return reader.ReadToEnd();
        }
    }

}
