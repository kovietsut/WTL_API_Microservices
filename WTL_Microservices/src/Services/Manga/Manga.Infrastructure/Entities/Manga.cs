using Contracts.Domains;
using Shared.Enums;

namespace Manga.Infrastructure.Entities
{
    public class Manga: EntityBase<long>
    {
        public DateTimeOffset? CreatedAt { get; set; }

        public DateTimeOffset? ModifiedAt { get; set; }

        public long? CreatedBy { get; set; }

        public long? ModifiedBy { get; set; }

        public string? Name { get; set; }

        public string? Preface { get; set; }

        public string? Type { get; set; }

        public string? Status { get; set; }

        public int? AmountOfReadings { get; set; }

        public string? CoverImage { get; set; }

        public LanguageEnum Language { get; set; }

        public bool? HasAdult { get; set; }

        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();

        public virtual ICollection<MangaGenre> MangasGenres { get; set; } = new List<MangaGenre>();

        public virtual ICollection<MangaInteraction> UserMangaInteractions { get; set; } = new List<MangaInteraction>();
    }
}
