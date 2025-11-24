using Microsoft.AspNetCore.Mvc;
using FinanceTracker.Api.Services;
using FinanceTracker.Api.Models;
using MongoDB.Bson;

namespace FinanceTracker.Api.Controllers
{
    /// <summary>
    /// Controller for managing recurring subscriptions.
    /// Supports creation, cancellation, and retrieval of subscriptions.
    /// </summary>
    [ApiController]
    [Route("api/subscriptions")]
    public class SubscriptionController : ControllerBase
    {
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionController(SubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Creates a new subscription for a user.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AddSubscription([FromBody] Subscription subscription)
        {
            var result = await _subscriptionService.AddSubscriptionAsync(subscription);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }

        /// <summary>
        /// Retrieves all active subscriptions for a user.
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserSubscriptions(string userId)
        {
            if (!ObjectId.TryParse(userId, out var objectId))
                return BadRequest("Invalid user ID.");

            var list = await _subscriptionService.GetSubscriptionsForUserAsync(objectId, onlyActive: true);
            return Ok(list);
        }

        /// <summary>
        /// Cancels a subscription (soft delete).
        /// </summary>
        [HttpDelete("{subscriptionId}")]
        public async Task<IActionResult> CancelSubscription(string subscriptionId)
        {
            if (!ObjectId.TryParse(subscriptionId, out var objectId))
                return BadRequest("Invalid subscription ID.");

            var success = await _subscriptionService.CancelSubscriptionAsync(objectId);

            if (!success)
                return NotFound("Subscription not found.");

            return Ok("Subscription canceled.");
        }

        /// <summary>
        /// Processes all due subscriptions and creates transactions.
        /// </summary>
        [HttpPost("process-due")]
        public async Task<IActionResult> ProcessDue()
        {
            var count = await _subscriptionService.ProcessDueSubscriptionsAsync(DateTime.UtcNow);
            return Ok($"{count} subscriptions processed.");
        }
    }
}
