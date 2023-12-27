using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.MangaGenre
{
    public class UpdateMangaGenreDto
    {
        public long MangaId { get; set; }
        public List<long> ListGenreId { get; set; }
    }
}
