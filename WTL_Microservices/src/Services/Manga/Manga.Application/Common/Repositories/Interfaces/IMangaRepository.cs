using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Manga;
using MangaEntity = Manga.Infrastructure.Entities.Manga;

namespace Manga.Application.Common.Repositories.Interfaces
{
    public interface IMangaRepository
    {
        Task<MangaEntity> GetMangaById(long mangaId);
        Task<IActionResult> GetManga(long userId);
        Task<IActionResult> GetListManga(int? pageNumber, int? pageSize, string? searchText);
        Task<IActionResult> CreateManga(CreateMangaDto model);
        Task<IActionResult> UpdateManga(long mangaId, UpdateMangaDto model);
        Task<IActionResult> RemoveSoftManga(long mangaId);
        Task<IActionResult> RemoveSoftListManga(string ids);
    }
}
