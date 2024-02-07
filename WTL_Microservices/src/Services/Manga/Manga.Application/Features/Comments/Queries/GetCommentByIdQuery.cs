using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Comments.Queries
{
    public class GetCommentByIdQuery(long id) : IRequest<IActionResult>
    {
        public long Id { get; private set; } = id;
    }

    public class GetCommentByIdQueryHandler : IRequestHandler<GetCommentByIdQuery, IActionResult>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger _logger;

        public GetCommentByIdQueryHandler(ICommentRepository commentRepository, ILogger logger)
        {
            _commentRepository = commentRepository;
            _logger = logger;
        }

        private const string MethodName = "GetCommentByIdQueryHandler";
        public async Task<IActionResult> Handle(GetCommentByIdQuery query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Id}");
            var genre = await _commentRepository.Get(query.Id);
            _logger.Information($"END: {MethodName} - Id: {query.Id}");
            return genre;
        }
    }
}
