using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Mangas.Commands
{
    public class DeleteMangaCommand(long id) : IRequest<IActionResult>
    {
        public long Id { get; private set; } = id;
    }

    public class DeleteMangaCommandHandler : IRequestHandler<DeleteMangaCommand, IActionResult>
    {
        private readonly IMangaRepository _mangaRepository;
        private readonly ILogger _logger;

        public DeleteMangaCommandHandler(IMangaRepository mangaRepository, ILogger logger)
        {
            _mangaRepository = mangaRepository;
            _logger = logger;
        }

        private const string MethodName = "DeleteMangaCommandHandler";
        public async Task<IActionResult> Handle(DeleteMangaCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Id}");
            var manga = await _mangaRepository.RemoveSoftManga(query.Id);
            _logger.Information($"END: {MethodName} - Id: {query.Id}");
            return manga;
        }
    }
}
