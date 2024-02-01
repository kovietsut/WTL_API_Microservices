using Contracts.Domains.Interfaces;
using Infrastructure.Common;
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
using Shared.DTOs.Chapter;
using Shared.DTOs.Manga;
using Shared.DTOs.MangaGenre;
using Shared.SeedWork;
using System;
using static Shared.DTOs.Chapter.CreateChapterDto;
using static Shared.DTOs.Chapter.UpdateChapterDto;

namespace Manga.Application.Common.Repositories
{
    public class ChapterRepository : RepositoryBase<Chapter, long, MangaContext>, IChapterRepository
    {
        private readonly ErrorCode _errorCodes;
        private readonly IMangaRepository _mangaRepository;
        private readonly IChapterImageRepository _chapterImageRepository;
        private readonly ISasTokenGenerator _sasTokenGenerator;

        public ChapterRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork, IMangaRepository mangaRepository,
            IChapterImageRepository chapterImageRepository, ISasTokenGenerator sasTokenGenerator, IOptions<ErrorCode> errorCode) :
            base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
            _mangaRepository = mangaRepository;
            _chapterImageRepository = chapterImageRepository;
            _sasTokenGenerator = sasTokenGenerator;
        }

        public Task<Chapter> GetChapterById(long chapterId) => FindByCondition(x => x.Id == chapterId).SingleOrDefaultAsync();

        public IActionResult GetList(int? pageNumber, int? pageSize, string? searchText)
        {
            try
            {
                pageNumber ??= 1; pageSize ??= 10;
                var list = FindAll().Where(x =>
                    x.IsEnabled == true && (searchText == null || x.Name.Contains(searchText.Trim())))
                .Select(x => new
                {
                    ChapterId = x.Id,
                    x.IsEnabled,
                    x.Name,
                    x.NumberOfChapter,
                    x.CreatedAt,
                    x.Status,
                    ThumbnailImage = _sasTokenGenerator.GenerateCoverImageUriWithSas(x.ThumbnailImage),
                    FavoriteChapters = x.MangaInteractions.Count(fav => fav.MangaId == x.MangaId && fav.Chapter.Id == x.Id && fav.InteractionType == "Favorite")
                }).Distinct();
                var listData = list.Skip(((int)pageNumber - 1) * (int)pageSize)
                    .Take((int)pageSize).OrderByDescending(x => x.ChapterId).ToList();
                if (list != null)
                {
                    var totalRecords = list.ToList().Count();
                    return JsonUtil.Success(listData, dataCount: totalRecords);
                }
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound,
                    "Empty List Data");
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Get(int chapterId)
        {
            try
            {
                if (chapterId == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "ChapterId is empty");
                }
                var chapter = await GetChapterById(chapterId);
                var manga = await _mangaRepository.GetMangaById((long)chapter.MangaId);
                if (manga.Type.Equals("TruyenTranh"))
                {
                    var list = await _chapterImageRepository.GetListImagesByChapter(chapterId);
                    return JsonUtil.Success(list);
                }
                return JsonUtil.Success(chapter.Content);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Create(CreateChapterDto model)
        {
            try
            {
                var validator = new CreateChapterValidator();
                var check = validator.Validate(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                Chapter chapter = new()
                {
                    IsEnabled = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = model.UserId,
                    NumberOfChapter = model.NumberOfChapter,
                    Name = model.Name,
                    HasDraft = model.HasDraft,
                    ThumbnailImage = model.ThumbnailImage,
                    PublishDate = DateTime.Parse(model.PublishDate),
                    HasComment = model.HasComment,
                    Content = model.Type.Equals("TruyenTranh") ? null : model.Content,
                    Language = model.Language,
                    MangaId = model.MangaId,
                    Status = model.Status
                };
                await CreateAsync(chapter);
                if (model.Type.Equals("TruyenTranh"))
                {
                    // Add List Chapter Images
                    await _chapterImageRepository.CreateList(chapter.Id, model.ImageList, (long)model.UserId);
                }
                return JsonUtil.Success(chapter.Id);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Update(int chapterId, UpdateChapterDto model)
        {
            try
            {
                var chapter = FindByCondition(x => x.Id == chapterId).FirstOrDefault();
                if (chapter == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Chapter does not exist");
                }
                var validator = new UpdateChapterValidator();
                var check = validator.Validate(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation,
                        check.Errors);
                }
                chapter.ModifiedAt = DateTime.Now;
                chapter.ModifiedBy = model.UserId;
                chapter.Name = model.Name.Trim();
                chapter.ThumbnailImage = model.ThumbnailImage.Trim();
                chapter.HasDraft = model.HasDraft;
                chapter.PublishDate = DateTime.Parse(model.PublishDate);
                chapter.HasComment = model.HasComment;
                chapter.Content = model.Type.Equals("TruyenTranh") ? null : model.Content.Trim();
                chapter.Language = model.Language;
                chapter.MangaId = model.MangaId;
                await UpdateAsync(chapter);
                if (model.Type.Equals("TruyenTranh"))
                {
                    // Remove List Old Chapter Images
                    await _chapterImageRepository.RemoveList(chapterId);
                    // Add List Chapter Images
                    await _chapterImageRepository.CreateList(chapter.Id, model.ImageList, (long)model.UserId);
                }
                return JsonUtil.Success(chapter.Id);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Approve(int chapterId)
        {
            try
            {
                var chapter = FindByCondition(x => x.Id == chapterId).FirstOrDefault();
                if (chapter == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Chapter does not exist");
                }
                chapter.ModifiedAt = DateTime.Now;
                //chapter.ModifiedBy = GetCurrentUserId();
                chapter.Status = "DuyetThanhCong";
                await UpdateAsync(chapter);
                return JsonUtil.Success(chapter.Id);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Reject(int chapterId)
        {
            try
            {
                var chapter = FindByCondition(x => x.Id == chapterId).FirstOrDefault();
                if (chapter == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Chapter does not exist");
                }
                chapter.ModifiedAt = DateTime.Now;
                //chapter.ModifiedBy = GetCurrentUserId();
                chapter.Status = "TuChoi";
                await UpdateAsync(chapter);
                return JsonUtil.Success(chapter.Id);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Disable(int chapterId)
        {
            try
            {
                if (chapterId == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "ChapterId cannot be null");
                }
                var chapter = FindByCondition(x => x.Id == chapterId).FirstOrDefault();
                if (chapter == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Chapter does not exist");
                }
                chapter.ModifiedAt = DateTime.Now;
                chapter.IsEnabled = false;
                await UpdateAsync(chapter);
                return JsonUtil.Success(chapter.Id);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> DeleteList(string ids)
        {
            await BeginTransactionAsync();
            try
            {
                if (ids.IsNullOrEmpty())
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Ids cannot be null");
                }
                var listIds = Util.SplitStringToArray(ids);
                var chapters = FindAll().Where(x => listIds.Contains(x.Id));
                if (chapters.Count() == 0)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Cannot get list chapters");
                }
                var listRemoved = chapters.Select(x => x.Id).ToList();
                await DeleteListAsync(chapters);
                await EndTransactionAsync();
                return JsonUtil.Success(listRemoved);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
            catch (Exception e)
            {
                return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.SystemError, e.Message);
            }
        }
    }
}
