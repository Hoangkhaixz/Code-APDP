using SIMSS.Interfaces;

namespace SIMSS.Services
{
    public class DefaultPasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return string.Concat(hash.Select(b => b.ToString("x2")));
            }
        }

        public bool Verify(string hash, string inputPassword)
        {
            var inputHash = Hash(inputPassword);
            return string.Equals(hash, inputPassword, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(hash, inputHash, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
