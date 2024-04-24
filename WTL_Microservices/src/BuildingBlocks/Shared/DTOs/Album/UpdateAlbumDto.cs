using FluentValidation;
using Shared.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.Album
{
    public class UpdateAlbumDto
    {
        public string Name { get; set; }
        public string CoverImage { get; set; }
    }

    public class UpdateAlbumValidator : AbstractValidator<UpdateAlbumDto>
    {
        public UpdateAlbumValidator()
        {
            RuleFor(x => x.Name).NotNull().WithMessage("Name is required")
                .NotEmpty().WithMessage("Name cannot be empty");
        }
    }
}
