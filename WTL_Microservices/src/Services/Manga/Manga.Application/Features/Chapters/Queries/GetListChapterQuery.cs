using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Chapters.Queries
{
    public class GetListChapterQuery(int? pageNumber, int? pageSize, string? searchText, long? mangaId) : IRequest<IActionResult>
    {
        public string? SearchText { get; set; } = searchText;
        public int? PageNumber { get; set; } = pageNumber;
        public int? PageSize { get; set; } = pageSize;
        public long? MangaId { get; set; } = mangaId;
    }

    public class GetListChapterQueryHandler : IRequestHandler<GetListChapterQuery, IActionResult>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ILogger _logger;

        public GetListChapterQueryHandler(IChapterRepository chapterRepository, ILogger logger)
        {
            _chapterRepository = chapterRepository;
            _logger = logger;
        }

        private const string MethodName = "GetListGenreQueryHandler";
        public async Task<IActionResult> Handle(GetListChapterQuery query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - SearchText: {query.SearchText}");
            var chapters = await _chapterRepository.GetList(query.PageNumber, query.PageSize, query.SearchText, query.MangaId);
            _logger.Information($"END: {MethodName} - SearchText: {query.SearchText}");
            return chapters;
        }
    }
}
