using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using AlbumEntity = Manga.Infrastructure.Entities.Album;
using Manga.Infrastructure.Persistence;
using Contracts.Domains.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Album;
using Shared.Common.Interfaces;
using Shared.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Shared.SeedWork;
using Microsoft.EntityFrameworkCore;
using Manga.Infrastructure.Entities;
using ServiceStack;

namespace Manga.Application.Common.Repositories
{
    public class AlbumRepository : RepositoryBase<AlbumEntity, long, MangaContext>, IAlbumRepository
    {
        private readonly ErrorCode _errorCodes;
        private readonly ISasTokenGenerator _sasTokenGenerator;
        private readonly IAlbumMangaRepository _albumMangaRepository;

        public AlbumRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork,
            IOptions<ErrorCode> errorCode, ISasTokenGenerator sasTokenGenerator, IAlbumMangaRepository albumMangaRepository) : base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
            _sasTokenGenerator = sasTokenGenerator;
            _albumMangaRepository = albumMangaRepository;
        }

        public async Task<IActionResult> CreateAlbum(CreateAlbumDto model)
        {
            try
            {
                var validator = new CreateAlbumValidator();
                var check = await validator.ValidateAsync(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                Album album = new()
                {
                    IsEnabled = true,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Name = model.Name != null ? model.Name.Trim() : model.Name,
                    CoverImage = model.CoverImage != null ? model.CoverImage.Trim() : model.CoverImage,
                };
                await CreateAsync(album);
                return JsonUtil.Success(album);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }

        public async Task<IActionResult> DeleteAlbum(long albumId)
        {
            try
            {
                var currentAlbum = await GetByIdAsync(albumId);
                if (currentAlbum == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Album does not exist");
                }
                currentAlbum.IsEnabled = false;
                await UpdateAsync(currentAlbum);
                return JsonUtil.Success(currentAlbum.Id);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }

        public Task<AlbumEntity> GetAlbumById(long albumId) => FindByCondition(x => x.Id == albumId).SingleOrDefaultAsync();

        public async Task<IActionResult> GetAlbum(long albumId, int? pageNumber, int? pageSize)
        {
            var album = await GetAlbumById(albumId);
            var mangaList = await _albumMangaRepository.GetListAlbumManga(albumId, pageNumber, pageSize);
            if (album == null)
            {
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Album does not exist");
            }
            var albumResult = new
            {
                album.Id,
                album.IsEnabled,
                album.CreatedBy,
                album.CreatedAt,
                album.Name,
                CoverImage = _sasTokenGenerator.GenerateCoverImageUriWithSas(album.CoverImage),
                mangaList
            };
            return JsonUtil.Success(albumResult);
        }

        public async Task<IActionResult> GetListAlbum(int? pageNumber, int? pageSize, string? searchText)
        {
            try
            {
                pageNumber ??= 1; pageSize ??= 10;
                var list = FindAll().Where(x => x.IsEnabled == true && (searchText == null || x.Name.Contains(searchText.Trim())))
                    .Select(x => new
                    {
                        AlbumId = x.Id,
                        x.IsEnabled,
                        x.CreatedAt,
                        x.CreatedBy,
                        x.Name,
                        CoverImage = _sasTokenGenerator.GenerateCoverImageUriWithSas(x.CoverImage)
                    });
                var listData = list.Skip(((int)pageNumber - 1) * (int)pageSize)
                    .Take((int)pageSize).OrderByDescending(x => x.AlbumId).ToList();
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

        public async Task<IActionResult> UpdateAlbum(long albumId, UpdateAlbumDto model)
        {
            try
            {
                // Validator
                var validator = new UpdateAlbumValidator();
                var check = await validator.ValidateAsync(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                // Album
                var currentAlbum = await GetByIdAsync(albumId);
                if (currentAlbum == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Album does not exist");
                }
                currentAlbum.ModifiedAt = DateTimeOffset.UtcNow;
                currentAlbum.ModifiedBy = model.CreatedBy;
                currentAlbum.Name = model.Name;
                currentAlbum.CoverImage = model.CoverImage;
                await UpdateAsync(currentAlbum);
                return JsonUtil.Success(currentAlbum);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }
    }
}
