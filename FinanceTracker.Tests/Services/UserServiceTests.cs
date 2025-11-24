using Xunit;
using Moq;
using MongoDB.Driver;
using FinanceTracker.Api.Services;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Helpers;
using BCrypt.Net;

namespace FinanceTracker.Tests.Services
{
    /// <summary>
    /// Testing UserService functions one by one
    /// Each test focuses on ONE function and what it returns
    /// </summary>
    public class UserServiceTests
    {
        private readonly Mock<IMongoClient> _mockMongoClient;
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly Mock<IMongoCollection<User>> _mockCollection;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            // Setup mocks (fake database)
            _mockMongoClient = new Mock<IMongoClient>();
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockCollection = new Mock<IMongoCollection<User>>();

            _mockMongoClient.Setup(c => c.GetDatabase("FinanceTrackerDB", null))
                .Returns(_mockDatabase.Object);
            _mockDatabase.Setup(d => d.GetCollection<User>("Users", null))
                .Returns(_mockCollection.Object);

            _userService = new UserService(_mockMongoClient.Object);
        }

        #region Testing AddUser() Function

        [Fact]
        public void AddUser_WithNewUsername_ShouldReturnSuccessTrue()
        {
            // Arrange
            var user = new User { Username = "newuser", Password = "Password123!" };

            // Mock: No existing user found
            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == null));

            // Act - Call the function
            ServiceResult result = _userService.AddUser(user);

            // Assert - Check what the function returned
            Assert.True(result.Success);
        }

        [Fact]
        public void AddUser_WithNewUsername_ShouldReturnCorrectMessage()
        {
            // Arrange
            var user = new User { Username = "testuser", Password = "Pass123!" };

            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == null));

            // Act
            ServiceResult result = _userService.AddUser(user);

            // Assert - Test the return statement
            Assert.Equal("User created successfully.", result.Message);
        }

        [Fact]
        public void AddUser_WithNewUsername_ShouldReturnUserData()
        {
            // Arrange
            var user = new User { Username = "john", Password = "Secure123!" };

            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == null));

            // Act
            ServiceResult result = _userService.AddUser(user);

            // Assert - Check if Data is returned
            Assert.NotNull(result.Data);
        }

        [Fact]
        public void AddUser_WithExistingUsername_ShouldReturnSuccessFalse()
        {
            // Arrange
            var existingUser = new User { Username = "duplicate", Password = "Pass1!" };
            var newUser = new User { Username = "duplicate", Password = "Pass2!" };

            // Mock: User already exists
            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == existingUser));

            // Act - Testing the if statement that checks for existing user
            ServiceResult result = _userService.AddUser(newUser);

            // Assert - Should hit the "if (existing != null)" branch
            Assert.False(result.Success);
        }

        [Fact]
        public void AddUser_WithExistingUsername_ShouldReturnDuplicateMessage()
        {
            // Arrange
            var existingUser = new User { Username = "taken", Password = "Pass1!" };
            var newUser = new User { Username = "taken", Password = "Pass2!" };

            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == existingUser));

            // Act
            ServiceResult result = _userService.AddUser(newUser);

            // Assert - Testing the return statement in the if block
            Assert.Equal("Username already exists.", result.Message);
        }

        [Fact]
        public void AddUser_ShouldHashPassword_NotStorePlaintext()
        {
            // Arrange
            var user = new User { Username = "secure", Password = "PlaintextPass123!" };
            User capturedUser = null;

            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == null));

            _mockCollection.Setup(c => c.InsertOne(It.IsAny<User>(), null, default))
                .Callback<User, InsertOneOptions, System.Threading.CancellationToken>((u, o, c) => capturedUser = u);

            // Act - Testing the line: user.Password = PasswordHelper.HashPassword(user.Password);
            _userService.AddUser(user);

            // Assert - Password should be hashed, not plaintext
            Assert.NotEqual("PlaintextPass123!", capturedUser.Password);
        }

        [Fact]
        public void AddUser_ShouldEncryptLastLoggedOn()
        {
            // Arrange
            var user = new User { Username = "timetest", Password = "Pass123!" };
            User capturedUser = null;

            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == null));

            _mockCollection.Setup(c => c.InsertOne(It.IsAny<User>(), null, default))
                .Callback<User, InsertOneOptions, System.Threading.CancellationToken>((u, o, c) => capturedUser = u);

            // Act - Testing: user.EncryptedLastLoggedOn = EncryptionHelper.Encrypt(...)
            _userService.AddUser(user);

            // Assert - Should have encrypted timestamp
            Assert.NotNull(capturedUser.EncryptedLastLoggedOn);
            Assert.NotEmpty(capturedUser.EncryptedLastLoggedOn);
        }

        #endregion

        #region Testing Login() Function

        [Fact]
        public void Login_WithNonExistentUsername_ShouldReturnSuccessFalse()
        {
            // Arrange
            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == null));

            // Act - Testing the if (user == null) condition
            ServiceResult result = _userService.Login("nonexistent", "Pass123!");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public void Login_WithNonExistentUsername_ShouldReturnUserNotFoundMessage()
        {
            // Arrange
            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == null));

            // Act - Testing the return statement when user is null
            ServiceResult result = _userService.Login("ghost", "Pass123!");

            // Assert
            Assert.Equal("User not found.", result.Message);
        }

        [Fact]
        public void Login_WithNonExistentUsername_ShouldReturn404StatusCode()
        {
            // Arrange
            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == null));

            // Act - Testing the StatusCode in return statement
            ServiceResult result = _userService.Login("nobody", "Pass123!");

            // Assert
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public void Login_WithWrongPassword_ShouldReturnSuccessFalse()
        {
            // Arrange
            var correctPassword = "CorrectPass123!";
            var wrongPassword = "WrongPass123!";
            var user = new User
            {
                Username = "user",
                Password = PasswordHelper.HashPassword(correctPassword),
                EncryptedLastLoggedOn = EncryptionHelper.Encrypt(DateTime.UtcNow.ToString("o"))
            };

            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == user));

            // Act - Testing the if (!isValid) condition
            ServiceResult result = _userService.Login("user", wrongPassword);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public void Login_WithWrongPassword_ShouldReturnInvalidPasswordMessage()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                Password = PasswordHelper.HashPassword("RightPass123!"),
                EncryptedLastLoggedOn = EncryptionHelper.Encrypt(DateTime.UtcNow.ToString("o"))
            };

            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == user));

            // Act
            ServiceResult result = _userService.Login("testuser", "WrongPass123!");

            // Assert - Testing return statement when password is invalid
            Assert.Equal("Invalid password.", result.Message);
        }

        [Fact]
        public void Login_WithWrongPassword_ShouldReturn401StatusCode()
        {
            // Arrange
            var user = new User
            {
                Username = "user",
                Password = PasswordHelper.HashPassword("Pass123!"),
                EncryptedLastLoggedOn = EncryptionHelper.Encrypt(DateTime.UtcNow.ToString("o"))
            };

            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == user));

            // Act
            ServiceResult result = _userService.Login("user", "BadPass!");

            // Assert
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public void Login_WithCorrectCredentials_ShouldReturnSuccessTrue()
        {
            // Arrange
            var password = "ValidPass123!";
            var user = new User
            {
                Username = "validuser",
                Password = PasswordHelper.HashPassword(password),
                EncryptedLastLoggedOn = EncryptionHelper.Encrypt(DateTime.UtcNow.ToString("o"))
            };

            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == user));

            _mockCollection.Setup(c => c.UpdateOne(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<UpdateDefinition<User>>(),
                null,
                default))
                .Returns(new UpdateResult.Acknowledged(1, 1, null));

            // Act - Testing successful login path
            ServiceResult result = _userService.Login("validuser", password);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public void Login_WithCorrectCredentials_ShouldReturnWelcomeMessage()
        {
            // Arrange
            var password = "Pass123!";
            var user = new User
            {
                Username = "john",
                Password = PasswordHelper.HashPassword(password),
                EncryptedLastLoggedOn = EncryptionHelper.Encrypt(DateTime.UtcNow.ToString("o"))
            };

            _mockCollection.Setup(c => c.Find(It.IsAny<FilterDefinition<User>>(), null))
                .Returns(Mock.Of<IFindFluent<User, User>>(f =>
                    f.FirstOrDefault(default) == user));

            _mockCollection.Setup(c => c.UpdateOne(
                It.IsAny<FilterDefinition<User>>(),
                It.IsAny<UpdateDefinition<User>>(),
                null,
                default))
                .Returns(new UpdateResult.Acknowledged(1, 1, null));

            // Act - Testing the return message
            ServiceResult result = _userService.Login("john", password);

            // Assert
            Assert.Contains("Welcome back", result.Message);
            Assert.Contains("john"