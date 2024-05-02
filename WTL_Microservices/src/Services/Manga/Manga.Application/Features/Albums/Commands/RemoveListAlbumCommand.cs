using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Albums.Commands
{
    public class RemoveListAlbumCommand(string ids) : IRequest<IActionResult>
    {
        public string Ids { get; set; } = ids;
    }

    public class RemoveListAlbumCommandHandler : IRequestHandler<RemoveListAlbumCommand, IActionResult>
    {
        private readonly IAlbumMangaRepository _albumMangaRepository;
        private readonly ILogger _logger;

        public RemoveListAlbumCommandHandler(IAlbumMangaRepository albumMangaRepository, ILogger logger)
        {
            _albumMangaRepository = albumMangaRepository;
            _logger = logger;
        }

        private const string MethodName = "DeleteListGenreCommandHandler";
        public async Task<IActionResult> Handle(RemoveListAlbumCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Ids}");
            var manga = await _albumMangaRepository.RemoveListFromAlbum(query.Ids);
            _logger.Information($"END: {MethodName} - Id: {query.Ids}");
            return manga;
        }
    }
}
