using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FinanceTracker.Api.Helpers;

namespace FinanceTracker.Tests.Helpers
{
    /// <summary>
    /// Tests for EncryptionHelper - Testing each function individually
    /// </summary>
    public class EncryptionHelperTests
    {
        #region Testing Encrypt() Function

        [Fact]
        public void Encrypt_WithValidString_ShouldReturnEncryptedString()
        {
            // Arrange
            string plainText = "Hello World";

            // Act
            string encrypted = EncryptionHelper.Encrypt(plainText);

            // Assert
            Assert.NotNull(encrypted);
            Assert.NotEmpty(encrypted);
            Assert.NotEqual(plainText, encrypted); // Encrypted should be different
        }

        [Fact]
        public void Encrypt_WithEmptyString_ShouldReturnEncryptedValue()
        {
            // Arrange
            string plainText = "";

            // Act
            string encrypted = EncryptionHelper.Encrypt(plainText);

            // Assert
            Assert.NotNull(encrypted);
            // Empty string can still be encrypted
        }

        [Fact]
        public void Encrypt_WithPassword_ShouldReturnBase64String()
        {
            // Arrange
            string password = "MySecurePassword123!";

            // Act
            string encrypted = EncryptionHelper.Encrypt(password);

            // Assert
            Assert.NotNull(encrypted);
            // Should be Base64 encoded (no spaces, valid characters)
            Assert.Matches("^[A-Za-z0-9+/=]+$", encrypted);
        }

        [Fact]
        public void Encrypt_SameInputTwice_ShouldReturnSameOutput()
        {
            // Arrange
            string plainText = "TestData";

            // Act
            string encrypted1 = EncryptionHelper.Encrypt(plainText);
            string encrypted2 = EncryptionHelper.Encrypt(plainText);

            // Assert
            // Since IV is hardcoded, same input = same output
            Assert.Equal(encrypted1, encrypted2);
        }

        [Fact]
        public void Encrypt_WithSpecialCharacters_ShouldEncryptSuccessfully()
        {
            // Arrange
            string plainText = "!@#$%^&*()_+{}|:<>?";

            // Act
            string encrypted = EncryptionHelper.Encrypt(plainText);

            // Assert
            Assert.NotNull(encrypted);
            Assert.NotEqual(plainText, encrypted);
        }

        [Fact]
        public void Encrypt_WithLongString_ShouldHandleCorrectly()
        {
            // Arrange
            string plainText = new string('A', 1000); // 1000 characters

            // Act
            string encrypted = EncryptionHelper.Encrypt(plainText);

            // Assert
            Assert.NotNull(encrypted);
            Assert.NotEmpty(encrypted);
        }

        #endregion

        #region Testing Decrypt() Function

        [Fact]
        public void Decrypt_WithValidEncryptedString_ShouldReturnOriginal()
        {
            // Arrange
            string original = "Hello World";
            string encrypted = EncryptionHelper.Encrypt(original);

            // Act
            string decrypted = EncryptionHelper.Decrypt(encrypted);

            // Assert
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void Decrypt_AfterEncryptingPassword_ShouldReturnOriginalPassword()
        {
            // Arrange
            string originalPassword = "MyPassword123!";
            string encrypted = EncryptionHelper.Encrypt(originalPassword);

            // Act
            string decrypted = EncryptionHelper.Decrypt(encrypted);

            // Assert
            Assert.Equal(originalPassword, decrypted);
        }

        [Fact]
        public void Decrypt_AfterEncryptingEmptyString_ShouldReturnEmptyString()
        {
            // Arrange
            string original = "";
            string encrypted = EncryptionHelper.Encrypt(original);

            // Act
            string decrypted = EncryptionHelper.Decrypt(encrypted);

            // Assert
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void Decrypt_WithInvalidBase64_ShouldThrowException()
        {
            // Arrange
            string invalidEncrypted = "This is not valid base64!!!";

            // Act & Assert
            Assert.Throws<FormatException>(() => EncryptionHelper.Decrypt(invalidEncrypted));
        }

        [Fact]
        public void Decrypt_WithSpecialCharacters_ShouldReturnOriginal()
        {
            // Arrange
            string original = "Test@#$%123";
            string encrypted = EncryptionHelper.Encrypt(original);

            // Act
            string decrypted = EncryptionHelper.Decrypt(encrypted);

            // Assert
            Assert.Equal(original, decrypted);
        }

        #endregion

        #region Testing Encrypt() and Decrypt() Together

        [Fact]
        public void EncryptThenDecrypt_WithUsername_ShouldReturnOriginal()
        {
            // Arrange
            string username = "john_doe";

            // Act
            string encrypted = EncryptionHelper.Encrypt(username);
            string decrypted = EncryptionHelper.Decrypt(encrypted);

            // Assert
            Assert.Equal(username, decrypted);
        }

        [Fact]
        public void EncryptThenDecrypt_WithDateTime_ShouldReturnOriginal()
        {
            // Arrange
            string dateTime = DateTime.UtcNow.ToString("o");

            // Act
            string encrypted = EncryptionHelper.Encrypt(dateTime);
            string decrypted = EncryptionHelper.Decrypt(encrypted);

            // Assert
            Assert.Equal(dateTime, decrypted);
        }

        [Theory]
        [InlineData("user1")]
        [InlineData("user@example.com")]
        [InlineData("123456")]
        [InlineData("Test Data With Spaces")]
        public void EncryptThenDecrypt_WithVariousInputs_ShouldReturnOriginal(string input)
        {
            // Arrange & Act
            string encrypted = EncryptionHelper.Encrypt(input);
            string decrypted = EncryptionHelper.Decrypt(encrypted);

            // Assert
            Assert.Equal(input, decrypted);
        }

        #endregion
    }
}