using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Genres.Queries
{
    public class GetListGenreQuery(int? pageNumber, int? pageSize, string? searchText) : IRequest<IActionResult>
    {
        public string? SearchText { get; set; } = searchText;
        public int? PageNumber { get; set; } = pageNumber;
        public int? PageSize { get; set; } = pageSize;
    }

    public class GetListGenreQueryHandler : IRequestHandler<GetListGenreQuery, IActionResult>
    {
        private readonly IGenreRepository _genreRepository;
        private readonly ILogger _logger;

        public GetListGenreQueryHandler(IGenreRepository genreRepository, ILogger logger)
        {
            _genreRepository = genreRepository;
            _logger = logger;
        }

        private const string MethodName = "GetListGenreQueryHandler";
        public async Task<IActionResult> Handle(GetListGenreQuery query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - SearchText: {query.SearchText}");
            var genres = await _genreRepository.GetListGenre(query.PageNumber, query.PageSize, query.SearchText);
            _logger.Information($"END: {MethodName} - SearchText: {query.SearchText}");
            return genres;
        }
    }
}
