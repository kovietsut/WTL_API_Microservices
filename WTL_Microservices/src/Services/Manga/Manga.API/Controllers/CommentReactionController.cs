using Manga.Application.Common.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.CommentReaction;

namespace Manga.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentReactionController : ControllerBase
    {
        private readonly ICommentReactionRepository _reactionRepository;

        public CommentReactionController(ICommentReactionRepository reactionRepository)
        {
            _reactionRepository = reactionRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CommentReactionDto model)
        {
            return await _reactionRepository.Create(model);
        }

        [HttpPut("{commentId}/{userId}")]
        public async Task<IActionResult> Update(long commentId, long userId, [FromBody] CommentReactionDto model)
        {
            return await _reactionRepository.Update(commentId, userId, model);
        }
    }
}
