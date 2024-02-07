using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Comments.Commands
{
    public class DeleteCommentCommand(long id) : IRequest<IActionResult>
    {
        public long Id { get; private set; } = id;
    }

    public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, IActionResult>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger _logger;

        public DeleteCommentCommandHandler(ICommentRepository commentRepository, ILogger logger)
        {
            _commentRepository = commentRepository;
            _logger = logger;
        }

        private const string MethodName = "DeleteCommentCommandHandler";
        public async Task<IActionResult> Handle(DeleteCommentCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Id}");
            var comment = await _commentRepository.Delete(query.Id);
            _logger.Information($"END: {MethodName} - Id: {query.Id}");
            return comment;
        }
    }
}
