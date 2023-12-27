using Manga.Application.Features.Mangas.Commands;
using Manga.Application.Features.Mangas.Queries;
using Manga.Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.DTOs.Manga;
using Shared.SeedWork;

namespace Manga.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MangaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MangaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var query = new GetMangaByIdQuery(id);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(int? pageNumber, int? pageSize, string? searchText)
        {
            var query = new GetListMangaQuery(pageNumber, pageSize, searchText);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> CreateManga([FromBody] CreateMangaDto model)
        {
            var query = new CreateMangaCommand()
            {
                CreatedBy = model.CreatedBy,
                Type = model.Type,
                Name = model.Name,
                Preface = model.Preface,
                AmountOfReadings = model.AmountOfReadings,
                CoverImage = model.CoverImage,
                Language = model.Language,
                HasAdult = model.HasAdult,
                ListGenreId = model.ListGenreId,
            };
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpPut("{mangaId}")]
        public async Task<IActionResult> Update(int mangaId, [FromBody] UpdateMangaDto model)
        {
            var query = new UpdateMangaCommand()
            {
                MangaId = mangaId,
                CreatedBy = model.CreatedBy,
                Type = model.Type,
                Name = model.Name,
                Status = model.Status,
                Preface = model.Preface,
                AmountOfReadings = model.AmountOfReadings,
                CoverImage = model.CoverImage,
                Language = model.Language,
                HasAdult = model.HasAdult,
                ListGenreId = model.ListGenreId,
            };
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpDelete("{mangaId}")]
        public async Task<IActionResult> Delete(long mangaId)
        {
            var query = new DeleteMangaCommand(mangaId);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteList(string ids)
        {
            var query = new DeleteListMangaCommand(ids);
            var result = await _mediator.Send(query);
            return result;
        }
    }
}
