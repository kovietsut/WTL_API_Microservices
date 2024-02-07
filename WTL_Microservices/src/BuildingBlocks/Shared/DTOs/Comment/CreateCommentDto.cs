using FluentValidation;
using Shared.DTOs.Chapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.Comment
{
    public class CreateCommentDto
    {
        public string Text { get; set; }
        public long? ParentCommentId { get; set; }
        public long ChapterId { get; set; }
    }

    public class CreateCommentValidator : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentValidator()
        {
            RuleFor(x => x.Text).NotNull().NotEmpty().WithMessage("Text cannot be empty");
            RuleFor(x => x.ChapterId).NotNull().NotEmpty().WithMessage("ChapterId can not be empty");
        }
    }
}
