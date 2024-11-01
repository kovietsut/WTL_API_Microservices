using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.AlbumManga;

namespace Manga.Application.Common.Repositories.Interfaces
{
    public interface IAlbumMangaRepository
    {
        Task<IActionResult> GetListAlbumManga(long albumId, int? pageNumber, int? pageSize);
        Task<IActionResult> SaveToAlbum(SaveToAlbumDto model);
        Task<IActionResult> RemoveFromAlbum(long albumMangaId);
        Task<IActionResult> RemoveListFromAlbum(string albumMangaIds);
    }
}
