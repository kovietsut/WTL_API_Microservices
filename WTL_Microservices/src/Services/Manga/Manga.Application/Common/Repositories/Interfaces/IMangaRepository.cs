using Microsoft.AspNetCore.Mvc;
using MangaEntity = Manga.Infrastructure.Entities.Manga;

namespace Manga.Application.Common.Repositories.Interfaces
{
    public interface IMangaRepository
    {
        Task<MangaEntity> GetMangaById(long mangaId);
        Task<IActionResult> GetManga(long userId);
        Task<IActionResult> GetListManga(int? pageNumber, int? pageSize, string? searchText);
    }
}
