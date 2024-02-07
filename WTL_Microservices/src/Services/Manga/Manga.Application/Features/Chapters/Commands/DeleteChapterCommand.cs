using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Chapters.Commands
{
    public class DeleteChapterCommand(long id) : IRequest<IActionResult>
    {
        public long Id { get; private set; } = id;
    }

    public class DeleteChapterCommandHandler : IRequestHandler<DeleteChapterCommand, IActionResult>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ILogger _logger;

        public DeleteChapterCommandHandler(IChapterRepository chapterRepository, ILogger logger)
        {
            _chapterRepository = chapterRepository;
            _logger = logger;
        }

        private const string MethodName = "DeleteGenreCommandHandler";
        public async Task<IActionResult> Handle(DeleteChapterCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Id}");
            var chapter = await _chapterRepository.Disable(query.Id);
            _logger.Information($"END: {MethodName} - Id: {query.Id}");
            return chapter;
        }
    }
}
