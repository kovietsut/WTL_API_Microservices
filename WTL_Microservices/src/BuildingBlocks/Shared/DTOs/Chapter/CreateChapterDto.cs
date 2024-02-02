using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;
using Shared.DTOs.ChapterImage;
using Shared.DTOs.Manga;
using Shared.Enums;
using Shared.SeedWork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.Chapter
{
    public class CreateChapterDto
    {
        public string? Name { get; set; }
        public string? Content { get; set; }
        public bool? HasDraft { get; set; }
        public string? ThumbnailImage { get; set; }
        public bool? HasComment { get; set; }
        public long? MangaId { get; set; }
        public LanguageEnum Language { get; set; }
        public int? NumberOfChapter { get; set; }
        public string? PublishDate { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }
        // Image
        public List<ChapterImageDto>? ImageList { get; set; }

        public class CreateChapterValidator : AbstractValidator<CreateChapterDto>
        {
            public CreateChapterValidator()
            {
                RuleFor(x => x.NumberOfChapter).NotNull()
                .NotEmpty().WithMessage("NumberOfChapter cannot be empty");
                RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Name cannot be empty");
                RuleFor(x => x.ThumbnailImage).NotNull().NotEmpty().WithMessage("ThumbnailImage cannot be empty");
                RuleFor(x => x.Language).NotNull().NotEmpty().WithMessage("LanguageId can not be empty");
                RuleFor(x => x.MangaId).GreaterThan(0).WithMessage("MangaId must be a positive number");
            }
        }
    }
}
