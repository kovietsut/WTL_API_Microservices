using Manga.Application.Features.Albums.Commands;
using Manga.Application.Features.Albums.Queries;
using Manga.Application.Features.Mangas.Commands;
using Manga.Application.Features.Mangas.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Album;
using Shared.DTOs.Manga;

namespace Manga.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AlbumController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AlbumController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpGet("get-list")]
        public async Task<IActionResult> GetList(int? pageNumber, int? pageSize, string? searchText)
        {
            var query = new GetListAlbumQuery(pageNumber, pageSize, searchText);
            var result = await _mediator.Send(query);
            return result;
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(long id)
        {
            var query = new GetAlbumByIdQuery(id);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAlbum([FromBody] CreateAlbumDto model)
        {
            var query = new CreateAlbumCommand()
            {
                Name = model.Name,
                CreatedBy = model.CreatedBy,
                CoverImage = model.CoverImage,
            };
            var result = await _mediator.Send(query);
            return result;
        }
    }
}
