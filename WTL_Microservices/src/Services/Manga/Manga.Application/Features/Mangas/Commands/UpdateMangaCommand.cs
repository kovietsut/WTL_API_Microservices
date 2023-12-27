using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.DTOs.Manga;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Features.Mangas.Commands
{
    public class UpdateMangaCommand : IRequest<IActionResult>
    {
        public long MangaId { get; set; }
        public long CreatedBy { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Preface { get; set; }
        public string Status { get; set; }
        public int AmountOfReadings { get; set; }
        public string CoverImage { get; set; }
        public LanguageEnum Language { get; set; }
        public bool HasAdult { get; set; }
        public List<long> ListGenreId { get; set; }
    }

    public class UpdateMangaCommandHandler : IRequestHandler<UpdateMangaCommand, IActionResult>
    {
        private readonly IMangaRepository _mangaRepository;
        private readonly ILogger _logger;

        public UpdateMangaCommandHandler(IMangaRepository mangaRepository, ILogger logger)
        {
            _mangaRepository = mangaRepository;
            _logger = logger;
        }

        private const string MethodName = "UpdateMangaCommandHandler";
        public async Task<IActionResult> Handle(UpdateMangaCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName}");
            var dto = new UpdateMangaDto()
            {
                CreatedBy = query.CreatedBy,
                Type = query.Type,
                Name = query.Name,
                Preface = query.Preface,
                Status = query.Status,
                AmountOfReadings = query.AmountOfReadings,
                CoverImage = query.CoverImage,
                Language = query.Language,
                HasAdult = query.HasAdult,
                ListGenreId = query.ListGenreId,
            };
            var mangas = await _mangaRepository.UpdateManga(query.MangaId, dto);
            _logger.Information($"END: {MethodName}");
            return mangas;
        }
    }
}
