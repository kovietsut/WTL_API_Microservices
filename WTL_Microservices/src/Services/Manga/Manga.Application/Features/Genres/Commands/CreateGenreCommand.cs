using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.DTOs.Genre;
using Shared.Enums;

namespace Manga.Application.Features.Genres.Commands
{
    public class CreateGenreCommand : IRequest<IActionResult>
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class CreateGenreCommandHandler : IRequestHandler<CreateGenreCommand, IActionResult>
    {
        private readonly IGenreRepository _genreRepository;
        private readonly ILogger _logger;

        public CreateGenreCommandHandler(IGenreRepository genreRepository, ILogger logger)
        {
            _genreRepository = genreRepository;
            _logger = logger;
        }

        private const string MethodName = "CreateGenreCommandHandler";
        public async Task<IActionResult> Handle(CreateGenreCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName}");
            var dto = new GenreDto()
            {
                IsEnabled = true,
                Name = query.Name,
            };
            var mangas = await _genreRepository.CreateGenre(dto);
            _logger.Information($"END: {MethodName}");
            return mangas;
        }
    }
}
