using Contracts.Domains;

namespace Manga.Infrastructure.Entities
{
    public class MangaGenre : EntityBase<long>
    {
        public long GenreId { get; set; }

        public long MangaId { get; set; }

        public virtual Genre Genre { get; set; } = null!;

        public virtual Manga Manga { get; set; } = null!;
    }
}
