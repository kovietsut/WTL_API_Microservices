using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Common.Interfaces;
using Shared.DTOs;
using Shared.DTOs.CommentReaction;
using Shared.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Manga.Application.Common.Repositories
{
    public class CommentReactionRepository : RepositoryBase<ChapterCommentReaction, long, MangaContext>, ICommentReactionRepository
    {
        private readonly ErrorCode _errorCodes;
        private readonly IBaseAuthService _baseAuthService;

        public CommentReactionRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork,
            IOptions<ErrorCode> errorCode, IBaseAuthService baseAuthService) : base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
            _baseAuthService = baseAuthService;
        }

        public Task<ChapterCommentReaction> GetById(long commentReactionId) => FindByCondition(x => x.Id == commentReactionId).SingleOrDefaultAsync();

        public async Task<IActionResult> GetCommentReaction(long commentReactionId)
        {
            var commentReaction = await GetById(commentReactionId);
            if (commentReaction == null)
            {
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "CommentReaction does not exist");
            }
            var commentReactionResult = new
            {
                commentReaction.Id,
                commentReaction.IsEnabled,
                commentReaction.IsLiked
            };
            return JsonUtil.Success(commentReactionResult);
        }

        //public async Task CreateChapterCommentReaction(bool isLiked, long userId, long chapterCommentId)
        //{
        //    var chapterCommentReaction = new ChapterCommentReaction()
        //    {
        //        IsEnabled = true,
        //        IsLiked = isLiked,
        //        UserId = userId,
        //        ChapterCommentId = chapterCommentId
        //    };
        //    await CreateAsync(chapterCommentReaction);
        //}
    }
}
