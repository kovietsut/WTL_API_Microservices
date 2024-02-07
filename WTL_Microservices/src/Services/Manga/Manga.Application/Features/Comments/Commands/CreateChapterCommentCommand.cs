using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.DTOs.Chapter;
using Shared.DTOs.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Features.Comments.Commands
{
    public class CreateChapterCommentCommand : IRequest<IActionResult>
    {
        public string Text { get; set; }
        public long? ParentCommentId { get; set; }
        public long ChapterId { get; set; }
    }

    public class CreateChapterCommentCommandHandler : IRequestHandler<CreateChapterCommentCommand, IActionResult>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger _logger;

        public CreateChapterCommentCommandHandler(ICommentRepository commentRepository, ILogger logger)
        {
            _commentRepository = commentRepository;
            _logger = logger;
        }

        private const string MethodName = "CreateChapterCommentCommandHandler";
        public async Task<IActionResult> Handle(CreateChapterCommentCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName}");
            var dto = new CreateCommentDto()
            {
                ChapterId = query.ChapterId,
                ParentCommentId = query.ParentCommentId,
                Text = query.Text,
            };
            var comment = await _commentRepository.Create(dto);
            _logger.Information($"END: {MethodName}");
            return comment;
        }
    }
}
