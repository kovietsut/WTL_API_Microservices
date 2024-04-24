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
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CoverImage {  get; set; }
        public long UserId { get; set; }
        public virtual ICollection<AlbumManga> AlbumsMangas { get; set; } = new List<AlbumManga>();
    }
}
