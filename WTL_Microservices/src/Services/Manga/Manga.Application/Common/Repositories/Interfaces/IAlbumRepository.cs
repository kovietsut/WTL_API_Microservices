using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Album;
using AlbumEntity = Manga.Infrastructure.Entities.Album;

namespace Manga.Application.Common.Repositories.Interfaces
{
    public interface IAlbumRepository
    {
        Task<IActionResult> GetAlbum(long albumId, int? pageNumber, int? pageSize);
        Task<AlbumEntity> GetAlbumById(long albumId);
        Task<IActionResult> GetListAlbum(int? pageNumber, int? pageSize, string? searchText);
        Task<IActionResult> CreateAlbum(CreateAlbumDto model);
        Task<IActionResult> UpdateAlbum(long albumId, UpdateAlbumDto model);
        Task<IActionResult> DeleteAlbum(long albumId);
    }
}
