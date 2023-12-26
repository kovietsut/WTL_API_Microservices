using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Mangas.Queries
{
    public class GetMangaByIdQuery(long id) : IRequest<IActionResult>
    {
        public long Id { get; private set; } = id;
    }

    public class GetMangaByIdQueryHandler : IRequestHandler<GetMangaByIdQuery, IActionResult>
    {
        private readonly IMangaRepository _mangaRepository;
        private readonly ILogger _logger;

        public GetMangaByIdQueryHandler(IMangaRepository mangaRepository, ILogger logger)
        {
            _mangaRepository = mangaRepository;
            _logger = logger;
        }

        private const string MethodName = "GetMangaByIdQueryHandler";
        public async Task<IActionResult> Handle(GetMangaByIdQuery query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Id}");
            var manga = await _mangaRepository.GetManga(query.Id);
            _logger.Information($"END: {MethodName} - Id: {query.Id}");
            return manga;
        }
    }
}
