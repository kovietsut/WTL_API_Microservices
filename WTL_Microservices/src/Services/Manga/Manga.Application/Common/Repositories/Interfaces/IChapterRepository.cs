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
        IActionResult GetList(int? pageNumber, int? pageSize, string? searchText);
        Task<IActionResult> Get(int chapterId);
        Task<IActionResult> Create(CreateChapterDto model);
        Task<IActionResult> Update(int chapterId, UpdateChapterDto model);
        Task<IActionResult> Approve(int chapterId);
        Task<IActionResult> Reject(int chapterId);
        Task<IActionResult> Disable(int chapterId);
        Task<IActionResult> DeleteList(string ids);
    }
}
