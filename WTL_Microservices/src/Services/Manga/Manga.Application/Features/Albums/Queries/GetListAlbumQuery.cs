using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;


namespace Manga.Application.Features.Albums.Queries
{
    public class GetListAlbumQuery(int? pageNumber, int? pageSize, string? searchText) : IRequest<IActionResult>
    {
        public string? SearchText { get; set; } = searchText;
        public int? PageNumber { get; set; } = pageNumber;
        public int? PageSize { get; set; } = pageSize;
    }

    public class GetListAlbumQueryHandler : IRequestHandler<GetListAlbumQuery, IActionResult>
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly ILogger _logger;
        public GetListAlbumQueryHandler(IAlbumRepository albumRepository, ILogger logger)
        {
            _albumRepository = albumRepository;
            _logger = logger;
        }

        private const string MethodName = "GetListAlbumQueryHandler";

        public async Task<IActionResult> Handle(GetListAlbumQuery query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - SearchText: {query.SearchText}");
            var albums = await _albumRepository.GetListAlbum(query.PageNumber, query.PageSize, query.SearchText);
            _logger.Information($"END: {MethodName} - SearchText: {query.SearchText}");
            return albums;
        }
    }
}
