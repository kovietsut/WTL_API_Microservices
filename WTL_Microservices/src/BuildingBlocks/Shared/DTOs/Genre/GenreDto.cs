using FluentValidation;

namespace Shared.DTOs.Genre
{
    public class GenreDto
    {
        public bool IsEnabled { get; set; } 
        public string Name { get; set; }
    }

    public class GenreValidator : AbstractValidator<GenreDto>
    {
        public GenreValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("Name is required")
                .NotEmpty().WithMessage("Name cannot be empty");
        }
    }
}
