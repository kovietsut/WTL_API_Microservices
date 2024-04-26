using Contracts.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Infrastructure.Entities
{
    public class AlbumManga : EntityBase<long>
    {
        public long MangaId { get; set; }
        public long AlbumId { get; set; }
        public DateTimeOffset? AddedDate { get; set; }
        public virtual Album Album { get; set; } = null!;
        public virtual Manga Manga { get; set; } = null!;
    }
}
