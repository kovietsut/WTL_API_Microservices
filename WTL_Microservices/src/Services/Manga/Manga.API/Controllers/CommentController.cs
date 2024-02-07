using Manga.Application.Features.Comments.Commands;
using Manga.Application.Features.Comments.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.DTOs.Comment;

namespace Manga.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [EnableRateLimiting("fixed")]
    public class CommentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CommentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var query = new GetCommentByIdQuery(id);
            var result = await _mediator.Send(query);
            return result;
        }

        [AllowAnonymous]
        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(int? pageNumber, int? pageSize, string? searchText, long? chapterId)
        {
            var query = new GetListCommentQuery(pageNumber, pageSize, searchText, chapterId);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CreateCommentDto model)
        {
            var query = new CreateChapterCommentCommand()
            {
                ChapterId = model.ChapterId,
                ParentCommentId = model.ParentCommentId,
                Text = model.Text,
            };
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpPut("{commentId}")]
        public async Task<IActionResult> Update(long commentId, [FromBody] UpdateCommentDto model)
        {
            var query = new UpdateChapterCommentCommand()
            {
                CommentId = commentId,
                ChapterId = model.ChapterId,
                ParentCommentId = model.ParentCommentId,
                Text = model.Text,
            };
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpDelete("{commentId}")]
        public async Task<IActionResult> Delete(long commentId)
        {
            var query = new DeleteCommentCommand(commentId);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteList(string ids)
        {
            var query = new DeleteListCommentCommand(ids);
            var result = await _mediator.Send(query);
            return result;
        }
    }
}
