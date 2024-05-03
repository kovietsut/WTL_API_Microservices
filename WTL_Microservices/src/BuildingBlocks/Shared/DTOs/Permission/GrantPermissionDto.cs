using FluentValidation;
using Shared.SeedWork;

namespace Shared.DTOs.Permission
{
    public class GrantPermissionDto
    {
        public long ActionId { get; set; }
        public long AlbumId { get; set; }
        public long MangaId { get; set; }
        public long UserId { get; set; }
        public string Type { get; set; }
    }

    public class GrantPermissionValidator : AbstractValidator<GrantPermissionDto>
    {
        public GrantPermissionValidator()
        {
            RuleFor(x => x.ActionId).NotEmpty().WithMessage("ActionId is required")
                .Must((actionId) => CheckValidationHelper.IsIntOrLong(actionId));
            RuleFor(x => x.AlbumId).NotEmpty().WithMessage("AlbumId is required")
                .Must((albumId) => CheckValidationHelper.IsIntOrLong(albumId));
            RuleFor(x => x.MangaId).NotEmpty().WithMessage("MangaId is required")
                .Must((mangaId) => CheckValidationHelper.IsIntOrLong(mangaId));
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required")
                .Must((userId) => CheckValidationHelper.IsIntOrLong(userId));
            RuleFor(x => x.Type).NotNull().WithMessage("Type is required")
                .NotEmpty().WithMessage("Type cannot be empty");
        }
    }
}
