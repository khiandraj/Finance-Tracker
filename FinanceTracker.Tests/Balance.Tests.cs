using Xunit;
using Moq;
using MongoDB.Driver;
using FinanceTracker.Api.Services;
using FinanceTracker.Api.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FinanceTracker.Api.Tests.Services
{
    public class BalanceServiceTests
    {
        private readonly Mock<IMongoClient> _mockClient;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly Mock<IMongoCollection<BalanceRecord>> _mockCollection;
        private readonly BalanceService _balanceService;

        public BalanceServiceTests()
        {
            _mockClient = new Mock<IMongoClient>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockCollection = new Mock<IMongoCollection<BalanceRecord>>();

            // MUST match BalanceService exactly: "FinanceTrackerDB"
            _mockClient.Setup(c => c.GetDatabase("FinanceTrackerDB", null))
                .Returns(_mockDatabase.Object);

            _mockDatabase.Setup(d => d.GetCollection<BalanceRecord>("UserBalances", null))
                .Returns(_mockCollection.Object);

            _balanceService = new BalanceService(_mockClient.Object);
        }

        // ---------- helpers for FindAsync ----------

        private void SetupFindReturnsNull()
        {
            var mockCursor = new Mock<IAsyncCursor<BalanceRecord>>();
            mockCursor.Setup(c => c.Current).Returns(Array.Empty<BalanceRecord>());
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(false);

            _mockCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BalanceRecord>>(),
                    It.IsAny<FindOptions<BalanceRecord, BalanceRecord>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);
        }

        private void SetupFindReturnsBalance(BalanceRecord balance)
        {
            var mockCursor = new Mock<IAsyncCursor<BalanceRecord>>();
            mockCursor.Setup(c => c.Current).Returns(new[] { balance });
            mockCursor.SetupSequence(c => c.MoveNextAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(true)
                      .ReturnsAsync(false);

            _mockCollection.Setup(c => c.FindAsync(
                    It.IsAny<FilterDefinition<BalanceRecord>>(),
                    It.IsAny<FindOptions<BalanceRecord, BalanceRecord>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockCursor.Object);
        }

        // ---------- tests ----------

        [Fact]
        public async Task GetBalanceByUserId_ReturnsBalance()
        {
            // Arrange
            var userId = "user123";
            var balance = new BalanceRecord { UserId = userId, Balance = 100m };
            SetupFindReturnsBalance(balance);

            // Act
            var result = await _balanceService.GetBalanceByUserId(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result!.UserId);
            Assert.Equal(100m, result.Balance);
        }

        [Fact]
        public async Task GetBalanceByUserId_ReturnsNull_WhenNotFound()
        {
            // Arrange
            SetupFindReturnsNull();

            // Act
            var result = await _balanceService.GetBalanceByUserId("nonexistent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateBalanceRecord_CreatesWithZeroBalance()
        {
            // Arrange
            var userId = "newuser";
            _mockCollection.Setup(c => c.InsertOneAsync(
                    It.IsAny<BalanceRecord>(),
                    null,
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _balanceService.CreateBalanceRecord(userId);

            // Assert
            Assert.Equal(userId, result.UserId);
            Assert.Equal(0m, result.Balance);
        }

        [Fact]
        public async Task Credit_AddsToBalance()
        {
            // Arrange
            var userId = "user123";
            var balance = new BalanceRecord { Id = "bal1", UserId = userId, Balance = 100m };
            SetupFindReturnsBalance(balance);

            _mockCollection.Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<BalanceRecord>>(),
                    It.IsAny<BalanceRecord>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<ReplaceOneResult>());

            // Act
            var result = await _balanceService.Credit(userId, 50m);

            // Assert
            Assert.Equal(150m, result.Balance);
        }

        [Fact]
        public async Task Debit_SubtractsFromBalance()
        {
            // Arrange
            var userId = "user123";
            var balance = new BalanceRecord { Id = "bal1", UserId = userId, Balance = 100m };
            SetupFindReturnsBalance(balance);

            _mockCollection.Setup(c => c.ReplaceOneAsync(
                    It.IsAny<FilterDefinition<BalanceRecord>>(),
                    It.IsAny<BalanceRecord>(),
                    It.IsAny<ReplaceOptions>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<ReplaceOneResult>());

            // Act
            var result = await _balanceService.Debit(userId, 30m);

            // Assert
            Assert.Equal(70m, result.Balance);
        }

        [Fact]
        public async Task DeleteBalanceRecord_ReturnsTrue_WhenDeleted()
        {
            // Arrange
            var mockResult = new Mock<DeleteResult>();
            mockResult.Setup(r => r.DeletedCount).Returns(1);

            _mockCollection.Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<BalanceRecord>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResult.Object);

            // Act
            var result = await _balanceService.DeleteBalanceRecord("user123");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteBalanceRecord_ReturnsFalse_WhenNotFound()
        {
            // Arrange
            var mockResult = new Mock<DeleteResult>();
            mockResult.Setup(r => r.DeletedCount).Returns(0);

            _mockCollection.Setup(c => c.DeleteOneAsync(
                    It.IsAny<FilterDefinition<BalanceRecord>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResult.Object);

            // Act
            var result = await _balanceService.DeleteBalanceRecord("nonexistent");

            // Assert
            Assert.False(result);
        }
    }
}
