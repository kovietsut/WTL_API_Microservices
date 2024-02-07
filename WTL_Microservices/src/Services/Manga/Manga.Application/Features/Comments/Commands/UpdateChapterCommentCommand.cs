using Manga.Application.Common.Repositories.Interfaces;
using Manga.Application.Features.Chapters.Commands;
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
    public class UpdateChapterCommentCommand : IRequest<IActionResult>
    {
        public long CommentId { get; set; }
        public string Text { get; set; }
        public long? ParentCommentId { get; set; }
        public long ChapterId { get; set; }
    }

    public class UpdateChapterCommentCommandHandler : IRequestHandler<UpdateChapterCommentCommand, IActionResult>
    {
        private readonly ICommentRepository _commentRepository;
        private readonly ILogger _logger;

        public UpdateChapterCommentCommandHandler(ICommentRepository commentRepository, ILogger logger)
        {
            _commentRepository = commentRepository;
            _logger = logger;
        }

        private const string MethodName = "UpdateChapterCommentCommandHandler";
        public async Task<IActionResult> Handle(UpdateChapterCommentCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName}");
            var dto = new UpdateCommentDto()
            {
                ChapterId = query.ChapterId,
                ParentCommentId = query.ParentCommentId,
                Text = query.Text,
            };
            var chapter = await _commentRepository.Update(query.CommentId, dto);
            _logger.Information($"END: {MethodName}");
            return chapter;
        }
    }
}
