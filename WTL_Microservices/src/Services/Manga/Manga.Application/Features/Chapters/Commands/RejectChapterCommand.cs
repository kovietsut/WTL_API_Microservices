using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Chapters.Commands
{
    public class RejectChapterCommand(long id) : IRequest<IActionResult>
    {
        public long Id { get; private set; } = id;
    }

    public class RejectChapterCommandHandler : IRequestHandler<RejectChapterCommand, IActionResult>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ILogger _logger;

        public RejectChapterCommandHandler(IChapterRepository chapterRepository, ILogger logger)
        {
            _chapterRepository = chapterRepository;
            _logger = logger;
        }

        private const string MethodName = "RejectChapterCommandHandler";
        public async Task<IActionResult> Handle(RejectChapterCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Id}");
            var genre = await _chapterRepository.Reject(query.Id);
            _logger.Information($"END: {MethodName} - Id: {query.Id}");
            return genre;
        }
    }
}
