using Contracts.Domains.Interfaces;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Authentication;
using User.API.Entities;

namespace User.API.Repositories.Interfaces
{
    public interface ITokenRepository: IRepositoryBase<Token, long>
    {
        Task<Token> GetTokenByRefreshToken(string refreshToken);
        // Làm basic
        Task<string> CreateToken(int userId);
        Task<string> IssueToken(string email);
        // Làm xịn hơn
        Task<TokenDto> GenerateToken(UserTokenDto model);
        Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(GoogleAuthModel model);
        DateTime ConvertUnixTimeToDateTime(long utcExpireDate);
    }
}
