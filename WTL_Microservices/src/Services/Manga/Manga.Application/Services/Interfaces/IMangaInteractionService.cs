using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.MangaChapterReaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Services.Interfaces
{
    public interface IMangaInteractionService
    {
        Task StoreMangaInteractionToDB();
    }
}
