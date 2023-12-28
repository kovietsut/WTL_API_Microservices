using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Genres.Queries
{
    public class GetGenreByIdQuery(long id) : IRequest<IActionResult>
    {
        public long Id { get; private set; } = id;
    }

    public class GetGenreByIdQueryHandler : IRequestHandler<GetGenreByIdQuery, IActionResult>
    {
        private readonly IGenreRepository _genreRepository;
        private readonly ILogger _logger;

        public GetGenreByIdQueryHandler(IGenreRepository genreRepository, ILogger logger)
        {
            _genreRepository = genreRepository;
            _logger = logger;
        }

        private const string MethodName = "GetGenreByIdQueryHandler";
        public async Task<IActionResult> Handle(GetGenreByIdQuery query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Id}");
            var genre = await _genreRepository.GetGenre(query.Id);
            _logger.Information($"END: {MethodName} - Id: {query.Id}");
            return genre;
        }
    }
}
