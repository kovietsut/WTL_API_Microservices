using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Common.Repositories.Interfaces
{
    public interface ICommentRepository
    {
        Task<IActionResult> GetList(int? pageNumber, int? pageSize, string? searchText, long? chapterId);
        Task<IActionResult> Get(long commendId);
        Task<IActionResult> Create(CreateCommentDto model);
        Task<IActionResult> Update(long commentId, UpdateCommentDto model);
        Task<IActionResult> Delete(long chapterCommentId);
        Task<IActionResult> DeleteList(string ids);
    }
}
