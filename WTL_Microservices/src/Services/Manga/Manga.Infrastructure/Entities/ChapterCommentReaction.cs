using Contracts.Domains;
using FluentValidation;
using Shared.DTOs.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Infrastructure.Entities
{
    public class ChapterCommentReaction : EntityBase<long>
    {
        public long UserId { get; set; }
        public long ChapterCommentId { get; set; }
        public bool IsLiked { get; set; }

        public virtual ChapterComment? ChapterComment { get; set; }
    }
}
