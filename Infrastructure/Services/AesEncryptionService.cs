using System.Security.Cryptography;
using System.Text;
using Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;

        public AesEncryptionService(IConfiguration configuration)
        {
            var keyString = configuration["Encryption:Key"]
                ?? throw new InvalidOperationException("Encryption:Key is required in configuration.");

            _key = SHA256.HashData(Encoding.UTF8.GetBytes(keyString));
        }

        public string Encrypt(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor();
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Prepend IV to cipher text
            var result = new byte[aes.IV.Length + cipherBytes.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

            return Convert.ToBase64String(result);
        }

        public string Decrypt(string cipherText)
        {
            var fullBytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = _key;

            var iv = new byte[aes.BlockSize / 8];
            var cipher = new byte[fullBytes.Length - iv.Length];
            Buffer.BlockCopy(fullBytes, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullBytes, iv.Length, cipher, 0, cipher.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}
