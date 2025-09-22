using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace FinanceTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FinanceController : ControllerBase
    {
        private static List<Transaction> _transactions = new();

        [HttpGet]
        public ActionResult<IEnumerable<Transaction>> GetTransactions()
        {
            return Ok(_transactions);
        }

        [HttpPost]
        public ActionResult<Transaction> AddTransaction([FromBody] Transaction transaction)
        {
            if (string.IsNullOrEmpty(transaction.Description) || transaction.Amount <= 0)
                return BadRequest("Invalid transaction data.");

            transaction.Id = _transactions.Count > 0 ? _transactions.Max(t => t.Id) + 1 : 1;
            transaction.Date = DateTime.Now;
            _transactions.Add(transaction);

            return Ok(transaction);
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteTransaction(int id)
        {
            var transaction = _transactions.FirstOrDefault(t => t.Id == id);
            if (transaction == null) return NotFound();

            _transactions.Remove(transaction);
            return NoContent();
        }
    }

    public class Transaction
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
