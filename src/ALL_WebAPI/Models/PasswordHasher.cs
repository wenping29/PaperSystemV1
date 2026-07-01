using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace PaperSystemApi.Models
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);
    }

    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
        private const char SaltDelimiter = ':';

        public string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            return $"{Convert.ToBase64String(salt)}{SaltDelimiter}{Convert.ToBase64String(hash)}";
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var parts = hashedPassword.Split(SaltDelimiter);
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            var providedHash = KeyDerivation.Pbkdf2(
                password: providedPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: Iterations,
                numBytesRequested: KeySize);

            return CryptographicOperations.FixedTimeEquals(hash, providedHash);
        }
    }
}
