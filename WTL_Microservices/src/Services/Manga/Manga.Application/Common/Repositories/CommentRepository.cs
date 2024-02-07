using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Common.Interfaces;
using Shared.DTOs;
using Shared.DTOs.Comment;
using Shared.SeedWork;

namespace Manga.Application.Common.Repositories
{
    public class CommentRepository : RepositoryBase<ChapterComment, long, MangaContext>, ICommentRepository
    {
        private readonly ErrorCode _errorCodes;
        private readonly IBaseAuthService _baseAuthService;

        public CommentRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork,
            IOptions<ErrorCode> errorCode, IBaseAuthService baseAuthService): base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
            _baseAuthService = baseAuthService;
        }

        public Task<ChapterComment> GetCommentById(long commentId) => FindByCondition(x => x.Id == commentId).SingleOrDefaultAsync();

        public async Task<IActionResult> GetList(int? pageNumber, int? pageSize, string? searchText, long? chapterId)
        {
            try
            {
                pageNumber ??= 1; pageSize ??= 10;
                var list = FindAll().Where(x =>
                    (chapterId == null || x.ChapterId ==  chapterId) &&
                    x.IsEnabled == true && (searchText == null || x.Text.Contains(searchText.Trim())))
                .Select(x => new
                {
                    CommentId = x.Id,
                    x.IsEnabled, x.ParentCommentId,
                    x.CreatedBy, x.CreatedAt,
                    x.ModifiedBy, x.ModifiedAt,
                    x.Text, x.ChapterId,
                    Likes = x.ChapterCommentReactions.Count(fav => fav.ChapterCommentId == x.Id && fav.IsLiked == true),
                    DisLikes = x.ChapterCommentReactions.Count(fav => fav.ChapterCommentId == x.Id && fav.IsLiked == false)
                });
                var listData = list.Skip(((int)pageNumber - 1) * (int)pageSize)
                    .Take((int)pageSize).OrderByDescending(x => x.CommentId).ToList();
                if (list != null)
                {
                    var totalRecords = list.ToList().Count();
                    return JsonUtil.Success(listData, dataCount: totalRecords);
                }
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Empty List Data");
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Get(long commendId)
        {
            try
            {
                if (commendId == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "CommendId is empty");
                }
                var comment = await GetCommentById(commendId);
                if ( comment == null) return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Commend is empty");
                return JsonUtil.Success(comment);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Create(CreateCommentDto model)
        {
            try
            {
                var validator = new CreateCommentValidator();
                var check = validator.Validate(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                ChapterComment chapterComment = new()
                {
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = _baseAuthService.GetCurrentUserId(),
                    ChapterId = model.ChapterId,
                    ParentCommentId = model.ParentCommentId,
                    Text = model.Text,
                };
                await CreateAsync(chapterComment);
                return JsonUtil.Success(chapterComment.Id);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Update(long commentId, UpdateCommentDto model)
        {
            try
            {
                var validator = new UpdateCommentValidator();
                var check = validator.Validate(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                var chapterComment = await GetByIdAsync(commentId);
                chapterComment.ModifiedAt = DateTime.UtcNow;
                chapterComment.ModifiedBy = _baseAuthService.GetCurrentUserId();
                chapterComment.ChapterId = model.ChapterId;
                chapterComment.ParentCommentId = model.ParentCommentId;
                chapterComment.Text = model.Text;
                await UpdateAsync(chapterComment);
                return JsonUtil.Success(chapterComment.Id);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Delete(long chapterCommentId)
        {
            try
            {
                var chapterComment = await GetByIdAsync(chapterCommentId);
                if (chapterComment == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "ChapterComment does not exist");
                }
                chapterComment.IsEnabled = false;
                await UpdateAsync(chapterComment);
                return JsonUtil.Success(chapterComment.Id);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }

        public async Task<IActionResult> DeleteList(string ids)
        {
            try
            {
                await BeginTransactionAsync();
                var list = new List<ChapterComment>();
                if (ids.IsNullOrEmpty())
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Ids cannot be null");
                }
                var listIds = Util.SplitStringToArray(ids);
                // List ChapterComments
                var chapterComments = FindAll().Where(x => listIds.Contains(x.Id));
                if (chapterComments == null || chapterComments.Count() == 0)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Cannot get list chapter comments");
                }
                foreach (var chapterComment in chapterComments)
                {
                    chapterComment.IsEnabled = false;
                    list.Add(chapterComment);
                }
                var listRemoved = chapterComments.Select(x => x.Id).ToList();
                if (list.Count != 0)
                {
                    await UpdateListAsync(list);
                }
                await EndTransactionAsync();
                return JsonUtil.Success(listRemoved);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }
    }
}
