using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace FinanceTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {

        private static List<User> _users = new();
        [HttpGet]


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

        [HttpGet("users")]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            return Ok(_users);
        }

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



    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
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
