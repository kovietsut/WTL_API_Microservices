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
        Task CreateMangaGenre(CreateMangaGenreDto model);
        Task UpdateMangaGenre(UpdateMangaGenreDto model);
    }
}
