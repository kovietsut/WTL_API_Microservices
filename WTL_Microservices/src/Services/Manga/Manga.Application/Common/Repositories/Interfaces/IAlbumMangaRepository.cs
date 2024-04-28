using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.AlbumManga;
using MangaEntity = Manga.Infrastructure.Entities.Manga;

namespace Manga.Application.Common.Repositories.Interfaces
{
    public interface IAlbumMangaRepository
    {
        Task<List<long>> GetListAlbumManga(long albumId);
        Task<IActionResult> SaveToAlbum(SaveToAlbumDto model);
        Task<IActionResult> RemoveFromAlbum(long albumMangaId);
        Task<IActionResult> RemoveListFromAlbum(string albumMangaIds);
    }
}
