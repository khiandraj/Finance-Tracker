using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

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
        private static List<User> _users = new();

        /// <summary>
        /// Adds a new user to the system.
        /// </summary>
        /// <param name="user">The user object containing <see cref="User.Username"/> and <see cref="User.Password"/>.</param>
        /// <returns>
        /// A response containing the created user if successful.
        /// Returns:
        /// - 400 Bad Request if username or password is missing.
        /// - 409 Conflict if the username already exists.
        /// - 200 OK with the user data if creation is successful.
        /// </returns>
        /// <example>
        /// POST: api/finance/adduser  
        /// Request Body:
        /// <code>
        /// {
        ///   "username": "alex123",
        ///   "password": "SecurePass123"
        /// }
        /// </code>
        /// </example>
        [HttpPost("adduser")]
        public ActionResult<User> AddUser([FromBody] User user)
        {
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                return BadRequest("Username and password are required.");
            if (_users.Any(u => u.Username == user.Username))
                return Conflict("Username already exists.");

            user.Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1;
            _users.Add(user);

            return Ok(user);
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

            var user = _users.FirstOrDefault(u => u.Username == loginRequest.Username);

            if (user == null)
                return NotFound("User not found.");

            if (user.Password != loginRequest.Password)
                return Unauthorized("Invalid password.");

            return Ok($"Welcome back, {user.Username}!");
        }
    }

    /// <summary>
    /// Represents a user account in the Finance Tracker system.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Username used to log in.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User's password (should be hashed and secured in production).
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }
}
