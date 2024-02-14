using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.MangaChapterReaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Common.Repositories.Interfaces
{
    public interface IMangaReactionRepository
    {
        Task<long> GetListMangaReaction(long mangaId);
        Task<IActionResult> CreateMangaInteraction(MangaInteractionDto model);
        Task<IActionResult> RemoveMangaInteraction(MangaInteractionDto model);
    }
}
