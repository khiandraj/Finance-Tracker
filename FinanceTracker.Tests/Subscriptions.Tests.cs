using Xunit;
using Moq;
using MongoDB.Driver;
using MongoDB.Bson;
using FinanceTracker.Api.Services;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FinanceTracker.Api.Tests.Services
{
    public class SubscriptionServiceTests
    {
        private readonly Mock<IMongoClient> _mockClient;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly Mock<IMongoCollection<Subscription>> _mockCollection;
        private readonly Mock<ITransactionService> _mockTransactionService;
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionServiceTests()
        {
            _mockClient = new Mock<IMongoClient>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockCollection = new Mock<IMongoCollection<Subscription>>();
            _mockTransactionService = new Mock<ITransactionService>();

            _mockClient.Setup(c => c.GetDatabase("FinanceTrackerDB", null))
                .Returns(_mockDatabase.Object);

            _mockDatabase.Setup(d => d.GetCollection<Subscription>("Subscriptions", null))
                .Returns(_mockCollection.Object);

            _subscriptionService = new SubscriptionService(_mockClient.Object, _mockTransactionService.Object);
        }

        [Fact]
        public async Task AddSubscriptionAsync_ValidSubscription_ReturnsSuccess()
        {
            // Arrange
            var subscription = new Subscription
            {
                UserId = ObjectId.GenerateNewId(),
                Name = "Netflix",
                Amount = 15.99m,
                Currency = "USD",
                Frequency = Frequency.Monthly,
                IsActive = true
            };

            _mockCollection.Setup(c => c.InsertOneAsync(
                It.IsAny<Subscription>(),
                null,
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _subscriptionService.AddSubscriptionAsync(subscription);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task GetSubscriptionsForUserAsync_ReturnsUserSubscriptions()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId();
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    UserId = userId,
                    Name = "Netflix",
                    Frequency = Frequency.Monthly,
                    Amount = 15.99m,
                    Currency = "USD",
                    IsActive = true
                },
                new Subscription
                {
                    UserId = userId,
                    Name = "Spotify",
                    Frequency = Frequency.Monthly,
                    Amount = 9.99m,
                    Currency = "USD",
                    IsActive = true
                }
            };

            var mockCursor = new Mock<IAsyncCursor<Subscription>>();
            mockCursor.Setup(c => c.Current).Returns(subscriptions);
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Subscription>>(),
                It.IsAny<FindOptions<Subscription>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            // Act
            var result = await _subscriptionService.GetSubscriptionsForUserAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task ProcessDueSubscriptionsAsync_ProcessesSubscriptions()
        {
            // Arrange
            var asOfDate = DateTime.UtcNow;
            var subscription = new Subscription
            {
                Id = ObjectId.GenerateNewId(),
                UserId = ObjectId.GenerateNewId(),
                Name = "Netflix",
                Amount = 15.99m,
                Currency = "USD",
                Frequency = Frequency.Monthly,
                NextPaymentUtc = DateTime.UtcNow.AddDays(-1),
                IsActive = true
            };

            var mockCursor = new Mock<IAsyncCursor<Subscription>>();
            mockCursor.Setup(c => c.Current).Returns(new[] { subscription });
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true)
                .ReturnsAsync(false);

            _mockCollection.Setup(c => c.FindAsync(
                It.IsAny<FilterDefinition<Subscription>>(),
                It.IsAny<FindOptions<Subscription>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);

            _mockTransactionService.Setup(t => t.RecordTransactionAsync(
                It.IsAny<ObjectId>(),
                It.IsAny<decimal>(),
                It.IsAny<string>(),
                It.IsAny<DateTime>(),
                It.IsAny<string>()))
                .ReturnsAsync(true);

            var mockUpdateResult = new Mock<UpdateResult>();
            mockUpdateResult.Setup(r => r.ModifiedCount).Returns(1);
            _mockCollection.Setup(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<Subscription>>(),
                It.IsAny<UpdateDefinition<Subscription>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockUpdateResult.Object);

            // Act
            var result = await _subscriptionService.ProcessDueSubscriptionsAsync(asOfDate);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task CancelSubscriptionAsync_SetsIsActiveFalse()
        {
            // Arrange
            var subscriptionId = ObjectId.GenerateNewId();
            var mockUpdateResult = new Mock<UpdateResult>();
            mockUpdateResult.Setup(r => r.ModifiedCount).Returns(1);

            _mockCollection.Setup(c => c.UpdateOneAsync(
                It.IsAny<FilterDefinition<Subscription>>(),
                It.IsAny<UpdateDefinition<Subscription>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockUpdateResult.Object);

            // Act
            var result = await _subscriptionService.CancelSubscriptionAsync(subscriptionId);

            // Assert
            Assert.True(result);
        }
    }
}