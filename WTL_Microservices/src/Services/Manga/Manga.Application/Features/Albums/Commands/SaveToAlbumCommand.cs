using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.DTOs.AlbumManga;

namespace Manga.Application.Features.Albums.Commands
{
    public class SaveToAlbumCommand : IRequest<IActionResult>
    {
        public long AlbumId { get; set; }
        public List<long> ListMangaId { get; set; }
    }

    public class SaveToAlbumCommandHandler : IRequestHandler<SaveToAlbumCommand, IActionResult>
    {
        private readonly IAlbumMangaRepository _albumMangaRepository;
        private readonly ILogger _logger;

        public SaveToAlbumCommandHandler(IAlbumMangaRepository albumMangaRepository, ILogger logger)
        {
            _albumMangaRepository = albumMangaRepository;
            _logger = logger;
        }

        private const string MethodName = "SaveToAlbumCommandHandler";
        public async Task<IActionResult> Handle(SaveToAlbumCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName}");
            var dto = new SaveToAlbumDto()
            {
                AlbumId = query.AlbumId,
                ListMangaId = query.ListMangaId
            };
            var album = await _albumMangaRepository.SaveToAlbum(dto);
            _logger.Information($"END: {MethodName}");
            return album;
        }
    }
}
