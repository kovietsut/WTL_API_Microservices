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
        Task<IActionResult> GetCommentReaction(long commentReactionId);
    }
}
