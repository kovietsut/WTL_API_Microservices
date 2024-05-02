using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Albums.Commands
{
    public class RemoveFromAlbumCommand(long id) : IRequest<IActionResult>
    {
        public long Id { get; private set; } = id;
    }

    public class RemoveFromAlbumCommandHandler : IRequestHandler<RemoveFromAlbumCommand, IActionResult>
    {
        private readonly IAlbumMangaRepository _albumMangaRepository;
        private readonly ILogger _logger;

        public RemoveFromAlbumCommandHandler(IAlbumMangaRepository albumMangaRepository, ILogger logger)
        {
            _albumMangaRepository = albumMangaRepository;
            _logger = logger;
        }

        private const string MethodName = "RemoveFromAlbumCommandHandler";
        public async Task<IActionResult> Handle(RemoveFromAlbumCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Id}");
            var genre = await _albumMangaRepository.RemoveFromAlbum(query.Id);
            _logger.Information($"END: {MethodName} - Id: {query.Id}");
            return genre;
        }
    }
}
