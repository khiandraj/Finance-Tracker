using Microsoft.AspNetCore.Mvc;
using FinanceTracker.Api.Services;
using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Controllers
{
    [ApiController]
    [Route("api/balance")]
    public class BalanceController : ControllerBase
    {
        private readonly BalanceService _service;

        public BalanceController(BalanceService service)
        {
            _service = service;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetBalance(string userId)
        {
            var balance = await _service.GetBalanceByUserId(userId);
            return balance == null ? NotFound() : Ok(balance);
        }

        [HttpPost("{userId}/credit")]
        public async Task<IActionResult> Credit(string userId, [FromBody] decimal amount)
        {
            var result = await _service.Credit(userId, amount);
            return Ok(result);
        }

        [HttpPost("{userId}/debit")]
        public async Task<IActionResult> Debit(string userId, [FromBody] decimal amount)
        {
            var result = await _service.Debit(userId, amount);
            return Ok(result);
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteRecord(string userId)
        {
            var deleted = await _service.DeleteBalanceRecord(userId);
            return deleted ? Ok("Record deleted.") : NotFound();
        }
    }
}
