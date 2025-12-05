using FinanceTracker.Api.Models;
using FinanceTracker.Api.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using System.Linq;
using System.Threading;
using Xunit;

namespace FinanceTracker.Api.Tests.Services
{
    public class UserServiceTests
    {
        private readonly Mock<IMongoClient> _mockClient;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly Mock<IMongoCollection<User>> _mockCollection;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockClient = new Mock<IMongoClient>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockCollection = new Mock<IMongoCollection<User>>();

            _mockClient.Setup(c => c.GetDatabase("FinanceTrackerDB", null))
                .Returns(_mockDatabase.Object);

            _mockDatabase.Setup(d => d.GetCollection<User>("Users", null))
                .Returns(_mockCollection.Object);

            _userService = new UserService(_mockClient.Object);
        }

        [Fact]
        public void AddUser_NewUser_ReturnsSuccess()
        {
            // Arrange
            var newUser = new User
            {
                Username = "testuser",
                Password = "password123",
                Role = "User"
            };

            var mockCursor = new Mock<IAsyncCursor<User>>();
            mockCursor.Setup(c => c.Current).Returns(Enumerable.Empty<User>());
            mockCursor.Setup(c => c.MoveNext(It.IsAny<CancellationToken>())).Returns(false);

            _mockCollection.Setup(c => c.FindSync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User>>(),
                It.IsAny<CancellationToken>()))
                .Returns(mockCursor.Object);

            _mockCollection.Setup(c => c.InsertOne(
                It.IsAny<User>(),
                null,
                It.IsAny<CancellationToken>()));

            // Act
            var result = _userService.AddUser(newUser);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("User created successfully.", result.Message);
        }

        [Fact]
        public void AddUser_DuplicateUsername_ReturnsFailure()
        {
            // Arrange
            var existingUser = new User
            {
                Id = ObjectId.GenerateNewId(),
                Username = "testuser",
                Password = "hashedpassword",
                Role = "User"
            };

            var newUser = new User
            {
                Username = "testuser",
                Password = "password123",
                Role = "User"
            };

            var mockCursor = new Mock<IAsyncCursor<User>>();
            mockCursor.Setup(c => c.Current).Returns(new[] { existingUser });
            mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);

            _mockCollection.Setup(c => c.FindSync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User>>(),
                It.IsAny<CancellationToken>()))
                .Returns(mockCursor.Object);

            // Act
            var result = _userService.AddUser(newUser);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Username already exists.", result.Message);
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var username = "testuser";
            var password = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var existingUser = new User
            {
                Id = ObjectId.GenerateNewId(),
                Username = username,
                Password = hashedPassword,
                Role = "User",
                EncryptedLastLoggedOn = "encrypteddata"
            };

            var mockCursor = new Mock<IAsyncCursor<User>>();
            mockCursor.Setup(c => c.Current).Returns(new[] { existingUser });
            mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);

            _mockCollection.Setup(c => c.FindSync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User>>(),
                It.IsAny<CancellationToken>()))
                .Returns(mockCursor.Object);

            var mockUpdateResult = new Mock<UpdateResult>();
            mockUpdateResult.Setup(r => r.ModifiedCount).Returns(1);
            _mockCollection.Setup(c => c.UpdateOne(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<UpdateDefinition<User>>(),
                It.IsAny<UpdateOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(mockUpdateResult.Object);

            // Act
            var result = _userService.Login(username, password);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("Welcome back", result.Message);
        }

        [Fact]
        public void Login_InvalidUsername_ReturnsFailure()
        {
            // Arrange
            var mockCursor = new Mock<IAsyncCursor<User>>();
            mockCursor.Setup(c => c.Current).Returns(Enumerable.Empty<User>());
            mockCursor.Setup(c => c.MoveNext(It.IsAny<CancellationToken>())).Returns(false);

            _mockCollection.Setup(c => c.FindSync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User>>(),
                It.IsAny<CancellationToken>()))
                .Returns(mockCursor.Object);

            // Act
            var result = _userService.Login("nonexistent", "password123");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public void Login_InvalidPassword_ReturnsFailure()
        {
            // Arrange
            var username = "testuser";
            var correctPassword = "password123";
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(correctPassword);

            var existingUser = new User
            {
                Id = ObjectId.GenerateNewId(),
                Username = username,
                Password = hashedPassword,
                Role = "User"
            };

            var mockCursor = new Mock<IAsyncCursor<User>>();
            mockCursor.Setup(c => c.Current).Returns(new[] { existingUser });
            mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);

            _mockCollection.Setup(c => c.FindSync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User>>(),
                It.IsAny<CancellationToken>()))
                .Returns(mockCursor.Object);

            // Act
            var result = _userService.Login(username, "wrongpassword");

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Invalid password.", result.Message);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public void GetAllUsers_ReturnsUserList()
        {
            // Arrange
            var users = new[]
            {
                new User
                {
                    Id = ObjectId.GenerateNewId(),
                    Username = "user1",
                    Password = "hash1",
                    Role = "User",
                    EncryptedLastLoggedOn = "encrypted1"
                }
            };

            var mockCursor = new Mock<IAsyncCursor<User>>();
            mockCursor.Setup(c => c.Current).Returns(users);
            mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<CancellationToken>()))
                .Returns(true)
                .Returns(false);

            _mockCollection.Setup(c => c.FindSync(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<FindOptions<User>>(),
                It.IsAny<CancellationToken>()))
                .Returns(mockCursor.Object);

            // Act
            var result = _userService.GetAllUsers();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}