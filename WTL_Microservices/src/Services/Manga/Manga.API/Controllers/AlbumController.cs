using Manga.Application.Features.Albums.Commands;
using Manga.Application.Features.Albums.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Album;
using Shared.DTOs.AlbumManga;

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
        public async Task<IActionResult> GetById(long id, int? pageNumber, int? pageSize)
        {
            var query = new GetAlbumByIdQuery(id, pageNumber, pageSize);
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

        [HttpPut("{albumId}")]
        public async Task<IActionResult> Update(long albumId, [FromBody] UpdateAlbumDto model)
        {
            var query = new UpdateAlbumCommand()
            {
                AlbumId = albumId,
                CreatedBy = model.CreatedBy,
                Name = model.Name,
                CoverImage = model.CoverImage,
            };
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpDelete("{albumId}")]
        public async Task<IActionResult> Delete(long albumId)
        {
            var query = new DeleteAlbumCommand(albumId);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpPost("save-to-album")]
        public async Task<IActionResult> SaveToAlbum([FromBody] SaveToAlbumDto model)
        {
            var query = new SaveToAlbumCommand()
            {
                AlbumId = model.AlbumId,
                ListMangaId = model.ListMangaId,
            };
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpDelete("remove-from-album/{albumMangaId}")]
        public async Task<IActionResult> RemoveFromAlbum(long albumMangaId)
        {
            var query = new RemoveFromAlbumCommand(albumMangaId);
            var result = await _mediator.Send(query);
            return result;
        }

        [HttpDelete("remove-list-from-album")]
        public async Task<IActionResult> RemoveListFromAlbum(string albumMangaIds)
        {
            var query = new RemoveListAlbumCommand(albumMangaIds);
            var result = await _mediator.Send(query); 
            return result;
        }
    }
}
