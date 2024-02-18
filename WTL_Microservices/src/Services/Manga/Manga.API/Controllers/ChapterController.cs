using Manga.Application.Features.Chapters.Commands;
using Manga.Application.Features.Chapters.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.DTOs.Chapter;

namespace Manga.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    //[EnableRateLimiting("fixed")]
    public class ChapterController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ChapterController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var query = new GetChapterByIdQuery(id);
            var result = await _mediator.Send(query);
            return result;
        }

        [AllowAnonymous]
        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(int? pageNumber, int? pageSize, string? searchText)
        {
            var query = new GetListChapterQuery(pageNumber, pageSize, searchText);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> CreateChapter([FromBody] CreateChapterDto model)
        {
            var query = new CreateChapterCommand()
            {
                Name = model.Name,
                Content = model.Content,
                HasDraft = model.HasDraft,
                ThumbnailImage = model.ThumbnailImage,
                HasComment = model.HasComment,
                MangaId = model.MangaId,
                Language = model.Language,
                NumberOfChapter = model.NumberOfChapter,
                PublishDate = model.PublishDate,
                Type = model.Type,
                Status = model.Status,
                ImageList = model.ImageList,
            };
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpPatch("{chapterId}/approve")]
        public async Task<IActionResult> Approve(long chapterId)
        {
            var query = new ApproveChapterCommand(chapterId);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpPatch("{chapterId}/reject")]
        public async Task<IActionResult> Reject(long chapterId)
        {
            var query = new RejectChapterCommand(chapterId);
            var result = await _mediator.Send(query);
            return result;
        }


        [HttpPut("{chapterId}")]
        public async Task<IActionResult> Update(long chapterId, [FromBody] UpdateChapterDto model)
        {
            var query = new UpdateChapterCommand()
            {
                Name = model.Name,
                ChapterId = chapterId,
                Content = model.Content,
                HasDraft = model.HasDraft,
                ThumbnailImage = model.ThumbnailImage,
                HasComment = model.HasComment,
                MangaId = model.MangaId,
                Language = model.Language,
                NumberOfChapter = model.NumberOfChapter,
                PublishDate = model.PublishDate,
                Type = model.Type,
                Status = model.Status,
                ImageList = model.ImageList,
            };
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpDelete("{chapterId}")]
        public async Task<IActionResult> Delete(long chapterId)
        {
            var query = new DeleteChapterCommand(chapterId);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteList(string ids)
        {
            var query = new DeleteListChapterCommand(ids);
            var result = await _mediator.Send(query);
            return result;
        }
    }
}
