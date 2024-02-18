using Manga.Application.Features.Genres.Commands;
using Manga.Application.Features.Genres.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.DTOs.Genre;

namespace Manga.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    //[EnableRateLimiting("fixed")]
    public class GenreController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GenreController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var query = new GetGenreByIdQuery(id);
            var result = await _mediator.Send(query);
            return result;
        }

        [AllowAnonymous]
        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(int? pageNumber, int? pageSize, string? searchText)
        {
            var query = new GetListGenreQuery(pageNumber, pageSize, searchText);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GenreDto model)
        {
            var query = new CreateGenreCommand()
            {
                IsEnabled = true,
                Name = model.Name,
            };
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpPut("{genreId}")]
        public async Task<IActionResult> Update(int genreId, [FromBody] GenreDto model)
        {
            var query = new UpdateGenreCommand()
            {
                GenreId = genreId,
                Name = model.Name,
                IsEnabled = true,
            };
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpDelete("{genreId}")]
        public async Task<IActionResult> Delete(long genreId)
        {
            var query = new DeleteGenreCommand(genreId);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteList(string ids)
        {
            var query = new DeleteListGenreCommand(ids);
            var result = await _mediator.Send(query);
            return result;
        }
    }
}
