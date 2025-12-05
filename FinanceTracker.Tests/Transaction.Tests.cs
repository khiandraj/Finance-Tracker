using Xunit;
using Moq;
using MongoDB.Driver;
using MongoDB.Bson;
using FinanceTracker.Api.Services;
using FinanceTracker.Api.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FinanceTracker.Api.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<IMongoClient> _mockClient;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly Mock<IMongoCollection<Transaction>> _mockCollection;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            _mockClient = new Mock<IMongoClient>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockCollection = new Mock<IMongoCollection<Transaction>>();

            _mockClient.Setup(c => c.GetDatabase("FinanceTrackerDB", null))
                .Returns(_mockDatabase.Object);

            _mockDatabase.Setup(d => d.GetCollection<Transaction>("Transactions", null))
                .Returns(_mockCollection.Object);

            _transactionService = new TransactionService(_mockClient.Object);
        }

        [Fact]
        public async Task RecordTransactionAsync_CreatesTransaction()
        {
            // Arrange
            var userId = ObjectId.GenerateNewId();
            var amount = 99.99m;
            var currency = "USD";
            var whenUtc = DateTime.UtcNow;
            var description = "Test transaction";

            _mockCollection.Setup(c => c.InsertOneAsync(
                It.IsAny<Transaction>(),
                null,
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _transactionService.RecordTransactionAsync(
                userId, amount, currency, whenUtc, description);

            // Assert
            Assert.True(result);
        }
    }
}
