using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Genres.Commands
{
    public class DeleteGenreCommand(long id) : IRequest<IActionResult>
    {
        public long Id { get; private set; } = id;
    }

    public class DeleteGenreCommandHandler : IRequestHandler<DeleteGenreCommand, IActionResult>
    {
        private readonly IGenreRepository _genreRepository;
        private readonly ILogger _logger;

        public DeleteGenreCommandHandler(IGenreRepository genreRepository, ILogger logger)
        {
            _genreRepository = genreRepository;
            _logger = logger;
        }

        private const string MethodName = "DeleteGenreCommandHandler";
        public async Task<IActionResult> Handle(DeleteGenreCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Id}");
            var genre = await _genreRepository.RemoveSoftGenre(query.Id);
            _logger.Information($"END: {MethodName} - Id: {query.Id}");
            return genre;
        }
    }
}
