using Contracts.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Domain.Entities
{
    public class ChapterImage : EntityBase<long>
    {
        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? ModifiedAt { get; set; }

        public long? CreatedBy { get; set; }

        public long? ModifiedBy { get; set; }

        public long? ChapterId { get; set; }

        public string? Name { get; set; }

        public string? FileSize { get; set; }

        public string? MimeType { get; set; }

        public string? FilePath { get; set; }

        public virtual Chapter? Chapter { get; set; }
    }
}
