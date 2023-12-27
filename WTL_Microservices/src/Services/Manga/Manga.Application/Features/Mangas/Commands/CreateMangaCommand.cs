using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.DTOs.Manga;
using Shared.Enums;

namespace Manga.Application.Features.Mangas.Commands
{
    public class CreateMangaCommand: IRequest<IActionResult>
    {
        public long CreatedBy { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Preface { get; set; }
        public int AmountOfReadings { get; set; }
        public string CoverImage { get; set; }
        public LanguageEnum Language { get; set; }
        public bool HasAdult { get; set; }
        public List<long> ListGenreId { get; set; }
    }

    public class CreateMangaCommandHandler : IRequestHandler<CreateMangaCommand, IActionResult>
    {
        private readonly IMangaRepository _mangaRepository;
        private readonly ILogger _logger;

        public CreateMangaCommandHandler(IMangaRepository mangaRepository, ILogger logger)
        {
            _mangaRepository = mangaRepository;
            _logger = logger;
        }

        private const string MethodName = "CreateMangaCommandHandler";
        public async Task<IActionResult> Handle(CreateMangaCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName}");
            var dto = new CreateMangaDto()
            {
                CreatedBy = query.CreatedBy,
                Type = query.Type,
                Name = query.Name,
                Preface = query.Preface,
                AmountOfReadings = query.AmountOfReadings,
                CoverImage = query.CoverImage,
                Language = query.Language,
                HasAdult = query.HasAdult,
                ListGenreId = query.ListGenreId,
            };
            var mangas = await _mangaRepository.CreateManga(dto);
            _logger.Information($"END: {MethodName}");
            return mangas;
        }
    }
}
