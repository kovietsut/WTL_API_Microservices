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
using Shared.DTOs.Comment;
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

        public async Task<IActionResult> Create(CommentReactionDto model)
        {
            try
            {
                var validator = new ChapterCommentReactionValidator();
                var check = validator.Validate(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                // Check if UserId & ChapterCommentId is the same
                var checkTheSame = Any(x => x.UserId == model.UserId && x.ChapterCommentId == model.ChapterCommentId);
                if (checkTheSame) return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Like existed");
                var chapterCommentReaction = new ChapterCommentReaction()
                {
                    IsEnabled = true,
                    IsLiked = model.IsLiked,
                    UserId = model.UserId,
                    ChapterCommentId = model.ChapterCommentId
                };
                await CreateAsync(chapterCommentReaction);
                return JsonUtil.Success(new
                {
                    chapterCommentReaction.Id, chapterCommentReaction.IsEnabled, chapterCommentReaction.UserId,
                    chapterCommentReaction.ChapterCommentId, chapterCommentReaction.IsLiked,
                });
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Update(long commentId, long userId, CommentReactionDto model)
        {
            try
            {
                var validator = new ChapterCommentReactionValidator();
                var check = validator.Validate(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                var commentReaction = FindByCondition(x => x.ChapterCommentId == commentId && x.UserId == userId).FirstOrDefault();
                commentReaction.IsLiked = model.IsLiked;
                await UpdateAsync(commentReaction);
                return JsonUtil.Success(commentReaction.Id);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }
    }
}
