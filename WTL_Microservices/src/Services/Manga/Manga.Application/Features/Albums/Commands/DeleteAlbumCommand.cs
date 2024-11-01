using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Albums.Commands
{
    public class DeleteAlbumCommand(long id) : IRequest<IActionResult>
    {
        public long Id { get; private set; } = id;
    }

    public class DeleteAlbumCommandHandler : IRequestHandler<DeleteAlbumCommand, IActionResult>
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly ILogger _logger;

        public DeleteAlbumCommandHandler(IAlbumRepository albumRepository, ILogger logger)
        {
            _albumRepository = albumRepository;
            _logger = logger;
        }

        private const string MethodName = "DeleteAlbumCommandHandler";
        public async Task<IActionResult> Handle(DeleteAlbumCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Id}");
            var album = await _albumRepository.DeleteAlbum(query.Id);
            _logger.Information($"END: {MethodName} - Id: {query.Id}");
            return album;
        }
    }
}
