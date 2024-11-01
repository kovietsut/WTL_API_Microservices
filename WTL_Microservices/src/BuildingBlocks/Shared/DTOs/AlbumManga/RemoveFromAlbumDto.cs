
namespace Shared.DTOs.AlbumManga
{
    public class RemoveFromAlbumDto
    {
        public long AlbumId { get; set; }
        public List<long> ListMangaId { get; set; }
    }
}
