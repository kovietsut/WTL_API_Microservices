using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Genre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Common.Repositories.Interfaces
{
    public interface IGenreRepository
    {
        Task<IActionResult> GetGenre(long genreId);
        Task<IActionResult> GetListGenre(int? pageNumber, int? pageSize, string? searchText);
        Task<IActionResult> CreateGenre(GenreDto model);
        Task<IActionResult> UpdateGenre(long genreId, GenreDto model);
        Task<IActionResult> RemoveSoftGenre(long genreId);
        Task<IActionResult> RemoveSoftListGenre(string ids);
    }
}
