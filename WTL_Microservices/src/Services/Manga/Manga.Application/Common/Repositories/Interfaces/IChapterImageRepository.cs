using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Chapter;
using Shared.DTOs.ChapterImage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Common.Repositories.Interfaces
{
    public interface IChapterImageRepository
    {
        Task<IActionResult> GetListChapterImage(int? pageNumber, int? pageSize, string? searchText);
        Task<IActionResult> GetChapterImage(long chapterImageId);
        Task<IActionResult> Create(ChaptermageListDto model);
        Task<IActionResult> Update(long chapterId, ChaptermageListDto model);
        Task<IActionResult> GetListImagesByChapter(long chapterId);
        Task CreateList(long chapterId, List<ChapterImageDto>? imageList);
        Task RemoveList(long chapterId);
    }
}
