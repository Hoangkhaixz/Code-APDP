using SIMSS.Interfaces;
using SIMSS.SimsDbContext.Entities;

namespace SIMSS.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        // Hàm Login đúng chuẩn async
        public async Task<Users?> LoginUserAsync(string username, string password)
        {
            // Lấy user theo username
            var user = await _userRepository.GetUserByUsernameAsync(username);

            // Kiểm tra user tồn tại
            if (user == null)
                return null;

            // Kiểm tra hợp lệ
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(user.PasswordHash))
                return null;

            // Dùng hasher để kiểm tra password lấy từ DB
            if (!_passwordHasher.Verify(user.PasswordHash, password))
                return null;

            return user;
        }

        // Hàm mã hóa password (SHA256)
        public static string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return string.Concat(hash.Select(b => b.ToString("x2")));
            }
        }

        public async Task<bool> CheckLoginAsync(string username, string password)
        {
            var user = await LoginUserAsync(username, password);
            return user != null;
        }
    }
}
