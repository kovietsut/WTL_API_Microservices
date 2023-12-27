using FluentValidation;
using Shared.DTOs.Manga;
using Shared.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.MangaGenre
{
    public class CreateMangaGenreDto
    {
        public long MangaId { get; set; }
        public List<long> ListGenreId { get; set; }
        
        public class CreateMangaGenreValidator : AbstractValidator<CreateMangaGenreDto>
        {
            public CreateMangaGenreValidator()
            {
                RuleFor(x => x.MangaId).NotEmpty().WithMessage("MangaId is required")
                    .Must((mangaId) => CheckValidationHelper.IsIntOrLong(mangaId));
                RuleFor(x => x.ListGenreId).NotNull().WithMessage("List Genere Id is required")
                    .NotEmpty().WithMessage("List Genere Id cannot be empty")
                    .Must(CheckValidationHelper.IsValidLongArray);
            }
        }
    }
}
