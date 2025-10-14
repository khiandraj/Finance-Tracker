using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using MongoDB.Bson;


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
        private readonly IMongoCollection<User> _usersCollection;

        public FinanceController()
        {
            var client = new MongoClient("mongodb://localhost:27017/?ssl=true");
            var database = client.GetDatabase("FinanceTrackerDB");
            _usersCollection = database.GetCollection<User>("Users");
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
        public ActionResult<IEnumerable<User>> GetUsers([FromQuery] string role = "User")
        {
            List<User> users;
            switch (role)
            {
                case "Global":
                    users = _usersCollection.Find(_ => true).ToList();
                    break;
                case "Developer":
                    users = _usersCollection.Find(u => u.Rold != "Global").ToList();
                    break;
                case "User":
                default:
                    users = _usersCollection.Find(u => u.Role == "User").ToList();
                    break;
            }
            return Ok(users.Select(u => new { u.Username, u.Role }));
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

            var user = _users.FirstOrDefault(u => u.Username == loginRequest.Username);

            if (user == null)
                return NotFound("User not found.");

            bool isValid = PasswordHelper.VerifyPassword(loginRequest.Password, user.Password);

            if (!isValid)
                return Unauthorized("Invalid password.");

            return Ok($"Welcome back, {user.Username}!");
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
        public string Role { get; set; } = "User";
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


}
