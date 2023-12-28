using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.DTOs.Genre;

namespace Manga.Application.Features.Genres.Commands
{
    public class UpdateGenreCommand : IRequest<IActionResult>
    {
        public long GenreId { get; set; }
        public bool IsEnabled { get; set; }
        public string Name { get; set; }
    }

    public class UpdateGenreCommandHandler : IRequestHandler<UpdateGenreCommand, IActionResult>
    {
        private readonly IGenreRepository _genreRepository;
        private readonly ILogger _logger;

        public UpdateGenreCommandHandler(IGenreRepository genreRepository, ILogger logger)
        {
            _genreRepository = genreRepository;
            _logger = logger;
        }

        private const string MethodName = "UpdateGenreCommandHandler";
        public async Task<IActionResult> Handle(UpdateGenreCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName}");
            var dto = new GenreDto()
            {
                IsEnabled = true,
                Name = query.Name,  
            };
            var genre = await _genreRepository.UpdateGenre(query.GenreId, dto);
            _logger.Information($"END: {MethodName}");
            return genre;
        }
    }
}
