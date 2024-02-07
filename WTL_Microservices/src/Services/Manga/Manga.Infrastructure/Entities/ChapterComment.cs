using Contracts.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Infrastructure.Entities
{
    public class ChapterComment: EntityBase<long>
    {
        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? ModifiedAt { get; set; }

        public long? CreatedBy { get; set; }

        public long? ModifiedBy { get; set; }

        public long? ChapterId { get; set; }

        public long? ParentCommentId { get; set; }

        public string? Text { get; set; }

        public virtual Chapter? Chapter { get; set; }

        public virtual ICollection<ChapterComment> InverseParentComment { get; set; } = new List<ChapterComment>();

        public virtual ChapterComment? ParentComment { get; set; }
        public virtual ICollection<ChapterCommentReaction> ChapterCommentReactions { get; set; } = new List<ChapterCommentReaction>();
    }
}
