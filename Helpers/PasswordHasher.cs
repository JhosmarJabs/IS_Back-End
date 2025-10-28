using System;
using System.Security.Cryptography;
using System.Text;

namespace IS_Back_End.Helpers
{
    public static class PasswordHasher
    {
        public static string Hash(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public static bool Verify(string input, string hash)
        {
            return Hash(input) == hash;
        }
    }
}
