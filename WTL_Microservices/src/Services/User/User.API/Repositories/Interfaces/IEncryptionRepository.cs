namespace User.API.Repositories.Interfaces
{
    public interface IEncryptionRepository
    {
        string CreateSalt();
        string CreateSalt(string code);
        string EncryptPassword(string username, string password);
        string HashSHA256(string value);
    }
}
