using FluentValidation;
using Shared.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.Album
{
    public class CreateAlbumDto
    {
        public string Name { get; set; }
        public long CreatedBy { get; set; }
        public string CoverImage { get; set; }
    }

    public class CreateAlbumValidator : AbstractValidator<CreateAlbumDto>
    {
        public CreateAlbumValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("Name is required")
                .NotEmpty().WithMessage("Name cannot be empty");
            RuleFor(x => x.CreatedBy).NotEmpty().WithMessage("CreatedBy is required")
                .Must((createdBy) => CheckValidationHelper.IsIntOrLong(createdBy));
        }
    }
}
