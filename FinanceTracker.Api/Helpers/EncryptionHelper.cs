using System.Security.Cryptography;
using System.Text;

namespace FinanceTracker.Api.Helpers
{
    public static class EncryptionHelper
    {
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("1234567890123456");

        public static string Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var bytes = Encoding.UTF8.GetBytes(plainText);

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(bytes, 0, bytes.Length);
                cs.FlushFinalBlock();
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static string Decrypt(string cipherText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var buffer = Convert.FromBase64String(cipherText);

            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            return reader.ReadToEnd();
        }
    }
}
