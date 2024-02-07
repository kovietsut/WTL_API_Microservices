using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.Comment
{
    public class UpdateCommentDto
    {
        public string Text { get; set; }
        public long? ParentCommentId { get; set; }
        public long ChapterId { get; set; }
    }

    public class UpdateCommentValidator : AbstractValidator<UpdateCommentDto>
    {
        public UpdateCommentValidator()
        {
            RuleFor(x => x.Text).NotNull().NotEmpty().WithMessage("Text cannot be empty");
            RuleFor(x => x.ChapterId).NotNull().NotEmpty().WithMessage("ChapterId can not be empty");
        }
    }
}
