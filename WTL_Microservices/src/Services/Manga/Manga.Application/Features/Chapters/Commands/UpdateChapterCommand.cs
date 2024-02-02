using Manga.Application.Common.Repositories.Interfaces;
using Manga.Application.Features.Genres.Commands;
using Manga.Infrastructure.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Shared.DTOs.Chapter;
using Shared.DTOs.ChapterImage;
using Shared.Enums;

namespace Manga.Application.Features.Chapters.Commands
{
    public class UpdateChapterCommand : IRequest<IActionResult>
    {
        public long ChapterId { get; set; }
        public string? Name { get; set; }
        public string? Content { get; set; }
        public bool? HasDraft { get; set; }
        public string? ThumbnailImage { get; set; }
        public bool? HasComment { get; set; }
        public long MangaId { get; set; }
        public LanguageEnum Language { get; set; }
        public int? NumberOfChapter { get; set; }
        public string? PublishDate { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        public List<ChapterImageDto>? ImageList { get; set; }
    }

    public class UpdateChapterCommandHandler : IRequestHandler<UpdateChapterCommand, IActionResult>
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ILogger _logger;

        public UpdateChapterCommandHandler(IChapterRepository chapterRepository, ILogger logger)
        {
            _chapterRepository = chapterRepository;
            _logger = logger;
        }

        private const string MethodName = "UpdateChapterCommandHandler";
        public async Task<IActionResult> Handle(UpdateChapterCommand query, CancellationToken cancellationToken)
        {
            _logger.Information($"BEGIN: {MethodName}");
            var dto = new UpdateChapterDto()
            {
                Name = query.Name,
                ChapterId= query.ChapterId,
                Content = query.Content,
                HasDraft = query.HasDraft,
                ThumbnailImage = query.ThumbnailImage,
                HasComment = query.HasComment,
                MangaId = query.MangaId,
                Language = query.Language,
                NumberOfChapter = query.NumberOfChapter,
                PublishDate = query.PublishDate,
                Type = query.Type,
                Status = query.Status,
                ImageList = query.ImageList,
            };
            var chapter = await _chapterRepository.Update(query.ChapterId, dto);
            _logger.Information($"END: {MethodName}");
            return chapter;
        }
    }
}
