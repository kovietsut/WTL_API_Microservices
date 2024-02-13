using FluentValidation;
using Shared.DTOs.Genre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.MangaChapterReaction
{
    public class MangaInteractionDto
    {
        public long? MangaId { get; set; }
        public long? ChapterId { get; set; }
        public string InteractionType { get; set; }

        public class MangaInteractionValidator : AbstractValidator<MangaInteractionDto>
        {
            public MangaInteractionValidator()
            {
                RuleFor(x => x.InteractionType).NotNull().WithMessage("InteractionType is required")
                    .NotEmpty().WithMessage("MangaId cannot be empty");
            }
        }
    }
}
