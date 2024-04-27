using FluentValidation;
using Shared.SeedWork;
namespace Shared.DTOs.AlbumManga
{
    public class SaveToAlbumDto
    {
        public long AlbumId { get; set; }
        public List<long> ListMangaId { get; set; }
    }

    public class SaveToAlbumValidator : AbstractValidator<SaveToAlbumDto>
    {
        public SaveToAlbumValidator()
        {
            RuleFor(x => x.AlbumId).NotEmpty().WithMessage("AlbumId is required")
                .Must((albumId) => CheckValidationHelper.IsIntOrLong(albumId));
            RuleFor(x => x.ListMangaId).NotEmpty().WithMessage("ListMangaId is required")
                .Must((listMangaId) => CheckValidationHelper.IsValidLongArray());
        }
    }
}
