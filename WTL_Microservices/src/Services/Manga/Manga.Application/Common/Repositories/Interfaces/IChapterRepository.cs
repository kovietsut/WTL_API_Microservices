using Manga.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Chapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Common.Repositories.Interfaces
{
    public interface IChapterRepository
    {
        Task<Chapter> GetChapterById(long chapterId);
        Task<IActionResult> GetList(int? pageNumber, int? pageSize, string? searchText, long? mangaId);
        Task<IActionResult> Get(long chapterId);
        Task<IActionResult> Create(CreateChapterDto model);
        Task<IActionResult> Update(long chapterId, UpdateChapterDto model);
        Task<IActionResult> Approve(long chapterId);
        Task<IActionResult> Reject(long chapterId);
        Task<IActionResult> Disable(long chapterId);
        Task<IActionResult> DeleteList(string ids);
    }
}
