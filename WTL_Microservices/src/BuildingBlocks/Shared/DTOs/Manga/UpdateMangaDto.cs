using FluentValidation;
using Shared.Enums;
using Shared.SeedWork;

namespace Shared.DTOs.Manga
{
    public class UpdateMangaDto
    {
        public long CreatedBy { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Preface { get; set; }
        public string Status { get; set; }
        public int AmountOfReadings { get; set; }
        public string CoverImage { get; set; }
        public LanguageEnum Language { get; set; }
        public List<long> ListGenreId { get; set; }
        public bool HasAdult { get; set; }
    }

    public class UpdateMangaValidator : AbstractValidator<UpdateMangaDto>
    {
        public UpdateMangaValidator()
        {
            RuleFor(x => x.CreatedBy).NotEmpty().WithMessage("CreatedBy is required")
                .Must((createdBy) => CheckValidationHelper.IsIntOrLong(createdBy));
            RuleFor(x => x.Type).NotNull().WithMessage("Type is required")
                .NotEmpty().WithMessage("Type not empty");
            RuleFor(x => x.Name).NotNull().WithMessage("Name is required")
                .NotEmpty().WithMessage("Name cannot be empty");
            RuleFor(x => x.Preface).NotNull().WithMessage("Preface is required")
                .NotEmpty().WithMessage("Preface cannot be empty");
            RuleFor(x => x.Language).NotNull().WithMessage("Language is required")
                .NotEmpty().WithMessage("Language cannot be empty");
            RuleFor(x => x.ListGenreId).NotNull().WithMessage("List Genere Id is required")
                .NotEmpty().WithMessage("List Genere Id cannot be empty")
                .Must(CheckValidationHelper.IsValidLongArray);
        }
    }
}
