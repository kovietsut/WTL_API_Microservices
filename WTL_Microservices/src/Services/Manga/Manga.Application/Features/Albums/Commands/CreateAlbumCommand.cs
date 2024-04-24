using Manga.Application.Common.Repositories.Interfaces;
using Manga.Application.Features.Mangas.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Manga;
using Serilog;
using Manga.Application.Common.Repositories;
using Shared.DTOs.Album;

namespace Manga.Application.Features.Albums.Commands
{
    public class CreateAlbumCommand : IRequest<IActionResult>
    {
        public string Name { get; set; }
        public long UserId { get; set; }
        public string CoverImage { get; set; }
    }

    public class CreateAlbumCommandHandler : IRequestHandler<CreateAlbumCommand, IActionResult>
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly ILogger _logger;

        public CreateAlbumCommandHandler(IAlbumRepository albumRepository, ILogger logger)
        {
            _albumRepository = albumRepository;
            _logger = logger;
        }

        private const string MethodName = "CreateAlbumCommandHandler";
        public async Task<IActionResult> Handle(CreateAlbumCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName}");
            var dto = new CreateAlbumDto()
            {
                
                Name = query.Name,
                UserId = query.UserId,
                CoverImage = query.CoverImage
            };
            var mangas = await _albumRepository.CreateAlbum(dto);
            _logger.Information($"END: {MethodName}");
            return mangas;
        }
    }
}
