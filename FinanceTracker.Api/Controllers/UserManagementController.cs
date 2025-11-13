using Microsoft.AspNetCore.Mvc;
using FinanceTracker.Api.Services;
using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Controllers
{
    ///<summary>
    /// Controller responsible for handling user registration, login,
    /// and retrieval — following Clean Architecture conventions.
    /// </summary>

    [ApiController]
    [Route("api/[controller]")]
    public class UserManagementController : ControllerBase
    {
        private readonly UserService _userService;

        public UserManagementController(UserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Registers a new user after validating input.
        /// </summary>
        [HttpPost("adduser")]
        public IActionResult AddUser([FromBody] User user)
        {
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
                return BadRequest("Username and password are required.");

            var result = _userService.AddUser(user);
            if (!result.Success)
                return Conflict(result.Message);

            return Ok(result.Data);
        }

        /// <summary>
        /// Logs in a user after validating input.
        /// </summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] User loginRequest)
        {
            if (string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
                return BadRequest("Username and password are required.");

            var result = _userService.Login(loginRequest.Username, loginRequest.Password);
            if (!result.Success)
                return StatusCode(result.StatusCode, result.Message);

            return Ok(result.Message);
        }

        /// <summary>
        /// Returns all registered users (without passwords).
        /// </summary>
        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var users = _userService.GetAllUsers();
            return Ok(users);
        }
    }
 
}

