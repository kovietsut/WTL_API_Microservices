using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.CommentReaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Common.Repositories.Interfaces
{
    public interface ICommentReactionRepository
    {
        Task<IActionResult> Create(CommentReactionDto model);
        Task<IActionResult> Update(long commentId, long userId, CommentReactionDto model);
    }
}
