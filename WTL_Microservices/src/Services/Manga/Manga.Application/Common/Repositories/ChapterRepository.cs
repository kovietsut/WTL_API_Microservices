using Contracts.Domains.Interfaces;
using EventBus.Messages.IntegrationEvents.Events;
using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Application.Services.Interfaces;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Common.Interfaces;
using Shared.DTOs;
using Shared.DTOs.Chapter;
using Shared.SeedWork;
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
        private readonly IBaseAuthService _baseAuthService;
        private readonly IMangaInteractionService _mangaInteractionService;
        private readonly IMangaReactionRepository _mangaReactionRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public ChapterRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork, IMangaRepository mangaRepository,
            IChapterImageRepository chapterImageRepository, ISasTokenGenerator sasTokenGenerator, IOptions<ErrorCode> errorCode,
            IBaseAuthService baseAuthService, IMangaInteractionService mangaInteractionService, IPublishEndpoint publishEndpoint,
            IMangaReactionRepository mangaReactionRepository) :
            base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
            _mangaRepository = mangaRepository;
            _chapterImageRepository = chapterImageRepository;
            _sasTokenGenerator = sasTokenGenerator;
            _baseAuthService = baseAuthService;
            _mangaInteractionService = mangaInteractionService;
            _publishEndpoint = publishEndpoint;
            _mangaReactionRepository = mangaReactionRepository;
        }

        public Task<Chapter> GetChapterById(long chapterId) => FindByCondition(x => x.Id == chapterId).SingleOrDefaultAsync();

        public async Task<IActionResult> GetList(int? pageNumber, int? pageSize, string? searchText, long? mangaId)
        {
            try
            {
                pageNumber ??= 1; pageSize ??= 10;
                var list = FindAll().Where(x => (mangaId == null || x.MangaId == mangaId) &&
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
                });
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

        public async Task<IActionResult> Get(long chapterId)
        {
            try
            {
                if (chapterId == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "ChapterId is empty");
                }
                var chapter = await GetChapterById(chapterId);
                if(chapter != null)
                {
                    var manga = await _mangaRepository.GetMangaById((long)chapter.MangaId);
                    var isFavorited = await _mangaInteractionService.CheckMangaChapterInteraction(manga.Id, chapter.Id, "Favorite");
                    if (manga.Type.Equals("TruyenTranh"))
                    {
                        var list = await _chapterImageRepository.GetListImagesByChapter(chapterId);
                        return JsonUtil.Success(new
                        {
                            MangaId = manga.Id,
                            MangaName = manga.Name,
                            ChapterId = chapter.Id,
                            ChapterName = chapter.Name,
                            images = list,
                            isFavorited
                        });
                    }
                    return JsonUtil.Success(new
                    {
                        MangaId = manga.Id,
                        MangaName = manga.Name,
                        ChapterId = chapter.Id,
                        ChapterName = chapter.Name,
                        chapter.Content,
                        isFavorited
                    });
                }
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Chapter does not exist");
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
                    CreatedBy = _baseAuthService.GetCurrentUserId(),
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
                // Get List User Followers
                var listMangaInteractions = await _mangaReactionRepository.GetListMangaFollowing((long)chapter.MangaId);
                var listUsers = listMangaInteractions.Select(x => x.UserId).ToList();
                // Publish Message || Get List Manga Favorites
                var eventMessage = new ChapterCreatedEvent()
                {
                    Id = chapter.Id,
                    UserId = chapter.CreatedBy,
                    ChapterName = chapter.Name,
                    PublishDate = chapter.PublishDate,
                    ListUser = listUsers,
                };
                await _publishEndpoint.Publish(eventMessage);
                if (model.Type.Equals("TruyenTranh"))
                {
                    // Add List Chapter Images
                    await _chapterImageRepository.CreateList(chapter.Id, model.ImageList);
                }
                return JsonUtil.Success(chapter.Id);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Update(long chapterId, UpdateChapterDto model)
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
                chapter.ModifiedBy = _baseAuthService.GetCurrentUserId();
                chapter.Name = model.Name.Trim();
                chapter.ThumbnailImage = model.ThumbnailImage.Trim();
                chapter.HasDraft = model.HasDraft;
                chapter.PublishDate = DateTime.Parse(model.PublishDate);
                chapter.HasComment = model.HasComment;
                chapter.Content = model.Type.Equals("TruyenTranh") ? null : model.Content.Trim();
                chapter.Language = model.Language;
                chapter.MangaId = model.MangaId;
                chapter.NumberOfChapter = model.NumberOfChapter;
                await UpdateAsync(chapter);
                if (model.Type.Equals("TruyenTranh"))
                {
                    // Remove List Old Chapter Images
                    await _chapterImageRepository.RemoveList(chapterId);
                    // Add List Chapter Images
                    await _chapterImageRepository.CreateList(chapter.Id, model.ImageList);
                }
                return JsonUtil.Success(chapter.Id);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Approve(long chapterId)
        {
            try
            {
                var chapter = FindByCondition(x => x.Id == chapterId).FirstOrDefault();
                if (chapter == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Chapter does not exist");
                }
                chapter.ModifiedAt = DateTime.Now;
                chapter.ModifiedBy = _baseAuthService.GetCurrentUserId();
                chapter.Status = "DuyetThanhCong";
                await UpdateAsync(chapter);
                return JsonUtil.Success(chapter.Id);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Reject(long chapterId)
        {
            try
            {
                var chapter = FindByCondition(x => x.Id == chapterId).FirstOrDefault();
                if (chapter == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Chapter does not exist");
                }
                chapter.ModifiedAt = DateTime.Now;
                chapter.ModifiedBy = _baseAuthService.GetCurrentUserId();
                chapter.Status = "TuChoi";
                await UpdateAsync(chapter);
                return JsonUtil.Success(chapter.Id);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task<IActionResult> Disable(long chapterId)
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
                var list = new List<Chapter>();
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
                foreach (var chapter in chapters)
                {
                    chapter.IsEnabled = false;
                    list.Add(chapter);
                }
                var listRemoved = chapters.Select(x => x.Id).ToList();
                if (list.Count != 0)
                {
                    await UpdateListAsync(list);
                }
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
