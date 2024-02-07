using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Comments.Queries
{
    public class GetListCommentQuery(int? pageNumber, int? pageSize, string? searchText, long? chapterId, long? mangaId) : IRequest<IActionResult>
    {
        public string? SearchText { get; set; } = searchText;
        public int? PageNumber { get; set; } = pageNumber;
        public int? PageSize { get; set; } = pageSize;
        public long? ChapterId { get; set; } = chapterId;
        public long? MangaId { get; set; } = mangaId;
    }

    public class GetListCommentQueryHandler : IRequestHandler<GetListCommentQuery, IActionResult>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger _logger;

        public GetListCommentQueryHandler(ICommentRepository commentRepository, ILogger logger)
        {
            _commentRepository = commentRepository;
            _logger = logger;
        }

        private const string MethodName = "GetListCommentQueryHandler";
        public async Task<IActionResult> Handle(GetListCommentQuery query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - SearchText: {query.SearchText}");
            var comments = await _commentRepository.GetList(query.PageNumber, query.PageSize, query.SearchText, query.ChapterId, query.MangaId);
            _logger.Information($"END: {MethodName} - SearchText: {query.SearchText}");
            return comments;
        }
    }
}
