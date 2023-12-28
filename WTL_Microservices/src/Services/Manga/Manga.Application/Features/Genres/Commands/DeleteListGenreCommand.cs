using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Genres.Commands
{
    public class DeleteListGenreCommand(string ids) : IRequest<IActionResult>
    {
        public string Ids { get; private set; } = ids;
    }

    public class DeleteListGenreCommandHandler : IRequestHandler<DeleteListGenreCommand, IActionResult>
    {
        private readonly IGenreRepository _genreRepository;
        private readonly ILogger _logger;

        public DeleteListGenreCommandHandler(IGenreRepository genreRepository, ILogger logger)
        {
            _genreRepository = genreRepository;
            _logger = logger;
        }

        private const string MethodName = "DeleteListGenreCommandHandler";
        public async Task<IActionResult> Handle(DeleteListGenreCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Ids}");
            var genre = await _genreRepository.RemoveSoftListGenre(query.Ids);
            _logger.Information($"END: {MethodName} - Id: {query.Ids}");
            return genre;
        }
    }
}
