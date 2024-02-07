using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.CommentReaction
{
    public class CommentReactionDto
    {
        public long UserId { get; set; }
        public long ChapterCommentId { get; set; }
        public bool IsLiked { get; set; }
    }

    public class ChapterCommentReactionValidator : AbstractValidator<CommentReactionDto>
    {
        public ChapterCommentReactionValidator()
        {
            RuleFor(x => x.UserId).NotNull().NotEmpty().WithMessage("UserId can not be empty");
        }
    }
}
