namespace NatournaServer.Interfaces.Authentication
{
    public interface IPasswordHashingService
    {
        string HashPassword(string password);

        bool VerifyPassword(string hashedPassword, string providedPassword);
    }
}
