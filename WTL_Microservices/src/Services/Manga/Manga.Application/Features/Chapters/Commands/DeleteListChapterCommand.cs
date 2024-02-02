using Manga.Application.Common.Repositories.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Manga.Application.Features.Chapters.Commands
{
    public class DeleteListChapterCommand(string ids) : IRequest<IActionResult>
    {
        public string Ids { get; private set; } = ids;
    }

    public class DeleteListChapterCommandHandler : IRequestHandler<DeleteListChapterCommand, IActionResult>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ILogger _logger;

        public DeleteListChapterCommandHandler(IChapterRepository chapterRepository, ILogger logger)
        {
            _chapterRepository = chapterRepository;
            _logger = logger;
        }

        private const string MethodName = "DeleteListChapterCommandHandler";
        public async Task<IActionResult> Handle(DeleteListChapterCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName} - Id: {query.Ids}");
            var genre = await _chapterRepository.DeleteList(query.Ids);
            _logger.Information($"END: {MethodName} - Id: {query.Ids}");
            return genre;
        }
    }
}
