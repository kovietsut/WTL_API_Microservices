using Contracts.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Infrastructure.Entities
{
    public class Album : EntityBase<long>
    {
        public string Name { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public long? CreatedBy { get; set; }
        public long? ModifiedBy { get; set; }
        public string? CoverImage {  get; set; }
        public virtual ICollection<AlbumManga> AlbumsMangas { get; set; } = new List<AlbumManga>();
    }
}
