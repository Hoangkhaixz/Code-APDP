using Microsoft.EntityFrameworkCore;
using SIMSS.Interfaces;
using SIMSS.SimsDbContext.Entities;

namespace SIMSS.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly SimsDbContext.SimsDbContext _context;
        public UserRepository(SimsDbContext.SimsDbContext dbcontext)
        {
            _context = dbcontext;
        }
        public async Task<Users> GetUserById(int id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserID == id);
        }

        public async Task<Users> GetUserByUsernameAsync(string username)
        {
            // So sánh username (case-sensitive vì SQL Server thường phân biệt case)
            // Nếu cần case-insensitive, cần cấu hình collation ở database level
            return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
    }
}
