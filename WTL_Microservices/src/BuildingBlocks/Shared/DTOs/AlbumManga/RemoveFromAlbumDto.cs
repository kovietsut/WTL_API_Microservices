using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs.AlbumManga
{
    public class RemoveFromAlbumDto
    {
        public long AlbumId { get; set; }
        public List<long> ListMangaId { get; set; }
    }
}
