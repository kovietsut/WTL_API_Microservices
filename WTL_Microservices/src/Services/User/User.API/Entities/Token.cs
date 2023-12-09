using Contracts.Domains;

namespace User.API.Entities
{
    public class Token: EntityBase<long>
    {
        public long UserId { get; set; }

        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        public DateTime? AccessTokenExpiration { get; set; }

        public DateTime? RefreshTokenExpiration { get; set; }

        public bool IsRevoked { get; set; }

        public string? JwtId { get; set; }

        public User User { get; set; } = null!;
    }
}
