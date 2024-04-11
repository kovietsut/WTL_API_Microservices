using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Authentication;
using User.API.Repositories.Interfaces;

namespace User.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class IssueTokenController : ControllerBase
    {
        private readonly ITokenRepository _iTokenRepository;

        public IssueTokenController(ITokenRepository iTokenRepository)
        {
            _iTokenRepository = iTokenRepository;
        }

        [AllowAnonymous]
        [HttpPost("issue-token")]
        public async Task<string> IssueToken([FromBody] EmailTokenDto model)
        {
            var token = await _iTokenRepository.IssueToken(model.Email);
            return token;
        }
    }
}
