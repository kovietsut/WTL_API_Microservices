using System.Security.Cryptography;
using System.Text;

namespace User.API.Infrastructure
{
    public class Encryption
    {
        public Encryption()
        {
        }

        public string CreateSalt()
        {
            var data = new byte[0x10];

            var cryptoServiceProvider = System.Security.Cryptography.RandomNumberGenerator.Create();
            cryptoServiceProvider.GetBytes(data);
            return Convert.ToBase64String(data);
        }

        public string CreateSalt(params object[] objs)
        {
            string salt = "@!";

            foreach (var obj in objs)
            {
                salt += obj;
            }

            salt += "!@";

            return salt;
        }

        public string HashSHA256(string value)
        {
            using (SHA256 hash = SHA256.Create())
            {
                return String.Concat(hash
                  .ComputeHash(Encoding.UTF8.GetBytes(value))
                  .Select(item => item.ToString("x2")));
            }
        }

        public string EncryptPassword(string password, string securityStamp)
        {
            return Encrypt(password, CreateSalt(new object[] { HashSHA256(securityStamp) }));
        }

        public string Encrypt(string value, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = string.Format("{0}{1}", salt, value);
                byte[] saltedPasswordAsBytes = Encoding.UTF8.GetBytes(saltedPassword);
                Console.WriteLine("sha256.ComputeHas: " + sha256.ComputeHash(saltedPasswordAsBytes));
                Console.WriteLine("ToBase64String: " + Convert.ToBase64String(sha256.ComputeHash(saltedPasswordAsBytes)));
                return Convert.ToBase64String(sha256.ComputeHash(saltedPasswordAsBytes));
            }
        }
    }
}
