using Manga.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.MangaGenre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Common.Repositories.Interfaces
{
    public interface IMangaGenreRepository
    {
        Task<List<string>> GetListMangaGenre(long mangaId);
        Task CreateMangaGenre(CreateMangaGenreDto model);
        Task RemoveSoftMangaGenre(long mangaId);
        Task UpdateMangaGenre(UpdateMangaGenreDto model);
    }
}
