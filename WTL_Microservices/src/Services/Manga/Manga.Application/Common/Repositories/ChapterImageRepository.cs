using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Application.Models;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.Common.Interfaces;
using Shared.DTOs;
using Shared.DTOs.ChapterImage;
using Shared.SeedWork;

namespace Manga.Application.Common.Repositories
{
    public class ChapterImageRepository : RepositoryBase<ChapterImage, long, MangaContext>, IChapterImageRepository
    {
        private readonly ErrorCode _errorCodes;
        private readonly ISasTokenGenerator _sasTokenGenerator;
        private readonly IBaseAuthService _baseAuthService;
        public ChapterImageRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork, IOptions<ErrorCode> errorCode, 
            ISasTokenGenerator sasTokenGenerator, IBaseAuthService baseAuthService) :
           base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
            _sasTokenGenerator = sasTokenGenerator;
            _baseAuthService = baseAuthService;
        }

        public Task<ChapterImage> GetChapterImageById(long chapterImageId) => FindByCondition(x => x.Id == chapterImageId).SingleOrDefaultAsync();

        public async Task<IActionResult> GetChapterImage(long chapterImageId)
        {
            var chapterImage = await GetChapterImageById(chapterImageId);
            if (chapterImage == null)
            {
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "ChapterImage does not exist");
            }
            var chapterImageResult = new
            {
                chapterImage.Id,
                chapterImage.IsEnabled,
                chapterImage.CreatedAt,
                chapterImage.CreatedBy,
                chapterImage.ModifiedAt,
                chapterImage.ModifiedBy,
                chapterImage.Name,
                chapterImage.FileSize,
                chapterImage.MimeType,
                chapterImage.FilePath
            };
            return JsonUtil.Success(chapterImageResult);
        }

        public async Task<IActionResult> GetListChapterImage(int? pageNumber, int? pageSize, string? searchText)
        {
            try
            {
                pageNumber ??= 1; pageSize ??= 10;
                var list = FindAll().Where(x => x.IsEnabled == true && (searchText == null || x.Name.Contains(searchText.Trim())))
                    .Select(x => new
                    {
                        ChapterImageId = x.Id,
                        x.IsEnabled,
                        x.CreatedAt,
                        x.CreatedBy,
                        x.ModifiedAt,
                        x.ModifiedBy,
                        x.Name,
                        x.FileSize,
                        x.MimeType,
                        x.FilePath,
                    });
                var listData = list.Skip(((int)pageNumber - 1) * (int)pageSize)
                    .Take((int)pageSize).OrderByDescending(x => x.ChapterImageId).ToList();
                if (list != null)
                {
                    var totalRecords = list.Count();
                    return JsonUtil.Success(listData, dataCount: totalRecords);
                }
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Empty List Data");
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, ex.Message);
            }
        }

        public async Task<List<BlobPathResponse>> GetListImagesByChapter(long chapterId)
        {
            var list = FindAll().Where(x => x.IsEnabled == true && x.ChapterId == chapterId)
                 .Select(x => new BlobPathResponse
                 {
                     IsEnabled = x.IsEnabled,
                     FilePath = _sasTokenGenerator.GenerateCoverImageUriWithSas(x.FilePath),
                 }).ToList();
            return list;
        }

        public async Task<IActionResult> Create(ChaptermageListDto model)
        {
            try
            {
                // Validator
                var listChapterImages = new List<ChapterImage>();
                model.ImageList.ForEach(item =>
                {
                    listChapterImages.Add(new ChapterImage
                    {
                        IsEnabled = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = item.CreatedBy,
                        ChapterId = model.ChapterId,
                        Name = item.Name,
                        FileSize = item.FileSize,
                        MimeType = item.MimeType,
                        FilePath = item.FilePath,
                    });
                });
                var result = await CreateListAsync(listChapterImages);
                return JsonUtil.Success(result);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }

        public async Task CreateList(long chapterId, List<ChapterImageDto>? imageList)
        {
            var listChapterImages = new List<ChapterImage>();
            imageList.ForEach(item =>
            {
                listChapterImages.Add(new ChapterImage
                {
                    IsEnabled = true,
                    CreatedAt = DateTime.Now,
                    CreatedBy = _baseAuthService.GetCurrentUserId(),
                    ChapterId = chapterId,
                    Name = item.Name,
                    FileSize = item.FileSize,
                    MimeType = item.MimeType,
                    FilePath = item.FilePath,
                });
            });
            await CreateListAsync(listChapterImages);
        }

        public async Task RemoveList(long chapterId)
        {
            // Remove List Old Chapter Images
            var listOldChapterImages = FindAll().Where(x => x.ChapterId == chapterId);
            await DeleteListAsync(listOldChapterImages);
        }

        public async Task<IActionResult> Update(long chapterId, ChaptermageListDto model)
        {
            try
            {
                // Remove List Old Chapter Images
                var listOldChapterImages = FindAll().Where(x => x.ChapterId == chapterId);
                await DeleteListAsync(listOldChapterImages);
                // Add List Chapter Images
                var listChapterImages = new List<ChapterImage>();
                model.ImageList.ForEach(item =>
                {
                    listChapterImages.Add(new ChapterImage
                    {
                        IsEnabled = true,
                        CreatedAt = DateTime.Now,
                        CreatedBy = item.CreatedBy,
                        ChapterId = chapterId,
                        Name = item.Name,
                        FileSize = item.FileSize,
                        MimeType = item.MimeType,
                        FilePath = item.FilePath,
                    });
                });
                var result = await CreateListAsync(listChapterImages);
                return JsonUtil.Success(result);
            }
            catch (UnauthorizedAccessException e)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, e.Message);
            }
        }
    }
}
