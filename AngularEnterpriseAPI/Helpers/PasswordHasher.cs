using System.Security.Cryptography;
using System.Text;

namespace AngularEnterpriseAPI.Helpers
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        public static string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            var randomBytes = new byte[length];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            var result = new StringBuilder(length);
            foreach (var byteValue in randomBytes)
            {
                result.Append(validChars[byteValue % validChars.Length]);
            }

            return result.ToString();
        }
    }
}
