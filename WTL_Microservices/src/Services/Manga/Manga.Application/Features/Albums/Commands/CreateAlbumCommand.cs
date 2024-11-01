using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.DTOs.Album;

namespace Manga.Application.Features.Albums.Commands
{
    public class CreateAlbumCommand : IRequest<IActionResult>
    {
        public string Name { get; set; }
        public long CreatedBy { get; set; }
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
                CreatedBy = query.CreatedBy,
                CoverImage = query.CoverImage
            };
            var album = await _albumRepository.CreateAlbum(dto);
            _logger.Information($"END: {MethodName}");
            return album;
        }
    }
}
