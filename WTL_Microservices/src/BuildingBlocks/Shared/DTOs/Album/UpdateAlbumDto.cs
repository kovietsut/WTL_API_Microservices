using FluentValidation;
using Shared.SeedWork;

namespace Shared.DTOs.Album
{
    public class UpdateAlbumDto
    {
        public string Name { get; set; }
        public long CreatedBy { get; set; }
        public string CoverImage { get; set; }
    }

    public class UpdateAlbumValidator : AbstractValidator<UpdateAlbumDto>
    {
        public UpdateAlbumValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("Name is required")
                .NotEmpty().WithMessage("Name cannot be empty");
            RuleFor(x => x.CreatedBy).NotEmpty().WithMessage("CreatedBy is required")
                .Must((createdBy) => CheckValidationHelper.IsIntOrLong(createdBy));
        }
    }
}
