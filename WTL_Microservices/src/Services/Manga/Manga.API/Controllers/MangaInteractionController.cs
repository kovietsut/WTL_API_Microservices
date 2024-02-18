using Manga.Application.Common.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.DTOs.MangaChapterReaction;

namespace Manga.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    //[EnableRateLimiting("fixed")]
    public class MangaInteractionController : ControllerBase
    {
        private readonly IMangaReactionRepository _mangaReactionRepository;

        public MangaInteractionController(IMangaReactionRepository mangaReactionRepository)
        {
            _mangaReactionRepository = mangaReactionRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MangaInteractionDto model)
        {
            return await _mangaReactionRepository.CreateMangaInteraction(model);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> Remove([FromBody] MangaInteractionDto model)
        {
            return await _mangaReactionRepository.RemoveMangaInteraction(model);
        }
    }
}
