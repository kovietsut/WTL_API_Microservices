using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.Manga
{
    public class UpdateMangaDto
    {
        public int MangaId { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Preface { get; set; }
        public string Status { get; set; }
        public int AmountOfReadings { get; set; }
        public string CoverImage { get; set; }
        public int LanguageId { get; set; }
        public List<int> ListGenreId { get; set; }
        public bool HasAdult { get; set; }
    }
}
