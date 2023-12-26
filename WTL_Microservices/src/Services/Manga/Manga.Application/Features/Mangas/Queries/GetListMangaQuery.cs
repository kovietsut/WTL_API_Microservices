using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using Shared.DTOs;

namespace Manga.Application.Features.Mangas.Queries
{
    public class GetListMangaQuery(int? pageNumber, int? pageSize, string? searchText) : IRequest<IActionResult>
    {
        public string? SearchText { get; set; } = searchText;
        public int? PageNumber { get; set;} = pageNumber;
        public int? PageSize { get; set;} = pageSize;
    }

    public class GetListMangaQueryHandler : IRequestHandler<GetListMangaQuery, IActionResult>
    {
        private readonly IMangaRepository _mangaRepository;
        private readonly ILogger _logger;

        public GetListMangaQueryHandler(IMangaRepository mangaRepository, ILogger logger)
        {
            _mangaRepository = mangaRepository;
            _logger = logger;
        }

        private const string MethodName = "GetListMangaQueryHandler";
        public async Task<IActionResult> Handle(GetListMangaQuery query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - SearchText: {query.SearchText}");
            var mangas = await _mangaRepository.GetListManga(query.PageNumber, query.PageSize, query.SearchText);
            _logger.Information($"END: {MethodName} - SearchText: {query.SearchText}");
            return mangas;
        }
    }
}
