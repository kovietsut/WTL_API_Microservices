using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Chapters.Commands
{
    public class ApproveChapterCommand(long id) : IRequest<IActionResult>
    {
        public long Id { get; private set; } = id;
    }

    public class ApproveChapterCommandHandler : IRequestHandler<ApproveChapterCommand, IActionResult>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ILogger _logger;

        public ApproveChapterCommandHandler(IChapterRepository chapterRepository, ILogger logger)
        {
            _chapterRepository = chapterRepository;
            _logger = logger;
        }

        private const string MethodName = "ApproveChapterCommandHandler";
        public async Task<IActionResult> Handle(ApproveChapterCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Id}");
            var genre = await _chapterRepository.Approve(query.Id);
            _logger.Information($"END: {MethodName} - Id: {query.Id}");
            return genre;
        }
    }
}
