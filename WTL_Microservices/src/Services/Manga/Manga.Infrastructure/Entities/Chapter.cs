using Contracts.Domains;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Infrastructure.Entities
{
    public class Chapter : EntityBase<long>
    {
        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? ModifiedAt { get; set; }

        public long? CreatedBy { get; set; }

        public long? ModifiedBy { get; set; }

        public int? NumberOfChapter { get; set; }

        public string? Name { get; set; }
        public string? Content { get; set; }

        public bool? HasDraft { get; set; }

        public string? ThumbnailImage { get; set; }

        public DateTimeOffset? PublishDate { get; set; }

        public bool? HasComment { get; set; }

        public LanguageEnum Language { get; set; }

        public long? MangaId { get; set; }

        public string? Status { get; set; }
        
        public virtual ICollection<ChapterComment> ChapterComments { get; set; } = new List<ChapterComment>();

        public virtual ICollection<ChapterImage> ChapterImages { get; set; } = new List<ChapterImage>();
        public virtual ICollection<MangaInteraction> MangaInteractions { get; set; } = new List<MangaInteraction>();

        public virtual Manga? Manga { get; set; }
    }
}
