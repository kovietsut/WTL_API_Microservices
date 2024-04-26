using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Album;
using Serilog;

namespace Manga.Application.Features.Albums.Commands
{
    public class UpdateAlbumCommand : IRequest<IActionResult>
    {
        public long AlbumId { get; set; }
        public string Name { get; set; }
        public long CreatedBy { get; set; }
        public string CoverImage { get; set; }
    }

    public class UpdateAlbumCommandHandler : IRequestHandler<UpdateAlbumCommand, IActionResult>
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly ILogger _logger;

        public UpdateAlbumCommandHandler(IAlbumRepository albumRepository, ILogger logger)
        {
            _albumRepository = albumRepository;
            _logger = logger;
        }

        private const string MethodName = "UpdateAlbumCommandHandler";
        public async Task<IActionResult> Handle(UpdateAlbumCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName}");
            var dto = new UpdateAlbumDto()
            {
                Name = query.Name,
                CreatedBy = query.CreatedBy,
                CoverImage = query.CoverImage
            };
            var album = await _albumRepository.UpdateAlbum(query.AlbumId, dto);
            _logger.Information($"END: {MethodName}");
            return album;
        }
    }
}
