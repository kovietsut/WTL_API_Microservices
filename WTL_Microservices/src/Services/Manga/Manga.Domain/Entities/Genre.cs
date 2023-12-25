using Contracts.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Domain.Entities
{
    public class Genre: EntityBase<long>
    {
        public string? Name { get; set; }
        public virtual ICollection<MangaGenre> MangasGenres { get; set; } = new List<MangaGenre>();
    }
}
