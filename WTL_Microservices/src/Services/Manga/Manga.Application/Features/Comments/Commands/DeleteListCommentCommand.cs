using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Comments.Commands
{
    public class DeleteListCommentCommand(string ids) : IRequest<IActionResult>
    {
        public string Ids { get; private set; } = ids;
    }

    public class DeleteListCommentCommandHandler : IRequestHandler<DeleteListCommentCommand, IActionResult>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger _logger;

        public DeleteListCommentCommandHandler(ICommentRepository commentRepository, ILogger logger)
        {
            _commentRepository = commentRepository;
            _logger = logger;
        }

        private const string MethodName = "DeleteListCommentCommandHandler";
        public async Task<IActionResult> Handle(DeleteListCommentCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Ids}");
            var genre = await _commentRepository.DeleteList(query.Ids);
            _logger.Information($"END: {MethodName} - Id: {query.Ids}");
            return genre;
        }
    }
}
