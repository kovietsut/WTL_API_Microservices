using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Serilog;
using Microsoft.AspNetCore.Mvc;


namespace Manga.Application.Features.Albums.Queries
{
    public class GetAlbumByIdQuery(long id, int? pageNumber, int? pageSize) : IRequest<IActionResult>
    {
        public long Id { get; private set; } = id;
        public int? PageNumber { get; set; } = pageNumber;
        public int? PageSize { get; set; } = pageSize;
    }

    public class GetAlbumByIdQueryHandler : IRequestHandler<GetAlbumByIdQuery, IActionResult>
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly ILogger _logger;

        public GetAlbumByIdQueryHandler(IAlbumRepository albumRepository, ILogger logger)
        {
            _albumRepository = albumRepository;
            _logger = logger;
        }

        private const string MethodName = "GetAlbumByIdQueryHandler";
        public async Task<IActionResult> Handle(GetAlbumByIdQuery query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Id}");
            var album = await _albumRepository.GetAlbum(query.Id, query.PageNumber, query.PageSize);
            _logger.Information($"END: {MethodName} - Id: {query.Id}");
            return album;
        }
    }
}
