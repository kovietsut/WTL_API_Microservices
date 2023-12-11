using Microsoft.Extensions.Options;
using Shared.DTOs.Encryption;
using User.API.Repositories.Interfaces;

namespace User.API.Repositories
{
    public class EncryptionRepository : IEncryptionRepository
    {
        public EncryptionRepository()
        {
        }

        private readonly Encryption _encryption;
        private readonly AppSetting _appSettings;

        public EncryptionRepository(IOptions<AppSetting> appSettings)
        {
            _encryption = new Encryption();
            _appSettings = appSettings.Value;
        }

        public string CreateSalt()
        {
            return _encryption.CreateSalt();
        }

        public string CreateSalt(string value)
        {
            return _encryption.CreateSalt(value, _appSettings.Salt);
        }

        public string EncryptPassword(string password, string securityStamp)
        {
            return _encryption.EncryptPassword(password, securityStamp);
        }

        public string HashSHA256(string value)
        {
            return _encryption.HashSHA256(value);
        }
    }
}
