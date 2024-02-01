using Contracts.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Infrastructure.Entities
{
    public class MangaInteraction : EntityBase<long>
    {
        public long? UserId { get; set; }

        public long? MangaId { get; set; }

        public long? ChapterId { get; set; }

        public string? InteractionType { get; set; }

        public virtual Manga? Manga { get; set; }
        public virtual Chapter? Chapter { get; set; }
    }
}
