namespace SIMSS.Interfaces
{
    public interface IUserRepository
    {
        Task<SimsDbContext.Entities.Users> GetUserByUsernameAsync(string username);
        Task<SimsDbContext.Entities.Users> GetUserById(int id);
    }
}
