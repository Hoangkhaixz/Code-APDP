namespace SIMSS.Interfaces
{
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string hash, string inputPassword);
    }
}
