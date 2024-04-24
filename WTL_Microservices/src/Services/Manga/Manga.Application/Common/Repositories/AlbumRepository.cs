using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using AlbumEntity = Manga.Infrastructure.Entities.Album;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Manga.Infrastructure.Persistence;
using Contracts.Domains.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Album;
using Manga.Application.Services.Interfaces;
using Shared.Common.Interfaces;
using Shared.DTOs;
using Manga.Application.Services;
using Shared.Common;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Shared.SeedWork;
using Microsoft.EntityFrameworkCore;
using Manga.Infrastructure.Entities;
using ServiceStack;
using Shared.DTOs.MangaGenre;
using Shared.DTOs.Manga;

namespace Manga.Application.Common.Repositories
{
    public class AlbumRepository : RepositoryBase<AlbumEntity, long, MangaContext>, IAlbumRepository
    {
        private readonly ErrorCode _errorCodes;
        private readonly ISasTokenGenerator _sasTokenGenerator;

        public AlbumRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork,
            IOptions<ErrorCode> errorCode, ISasTokenGenerator sasTokenGenerator) : base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
            _sasTokenGenerator = sasTokenGenerator;
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

        public Task<IActionResult> DeleteAlbum(long albumId)
        {
            throw new NotImplementedException();
        }

        public Task<AlbumEntity> GetAlbumById(long albumId) => FindByCondition(x => x.Id == albumId).SingleOrDefaultAsync();

        public async Task<IActionResult> GetAlbum(long userId)
        {
            var album = await GetAlbumById(userId);
            if(album == null)
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
            //try
            //{
            //    // Validator
            //    var validator = new UpdateAlbumValidator();
            //    var check = await validator.ValidateAsync(model);
            //    if (!check.IsValid)
            //    {
            //        return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
            //    }
            //    // Manga
            //    var currentAlbum = await GetByIdAsync(albumId);
            //    if (currentAlbum == null)
            //    {
            //        return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Album does not exist");
            //    }
            //    currentAlbum.ModifiedAt = DateTimeOffset.UtcNow;
            //    currentAlbum.ModifiedBy = model.CreatedBy;
            //    currentAlbum.Name = model.Name;
            //    currentAlbum.Preface = model.Preface;
            //    currentAlbum.Type = model.Type;
            //    currentAlbum.Status = model.Status;
            //    currentAlbum.AmountOfReadings = model.AmountOfReadings;
            //    currentAlbum.CoverImage = model.CoverImage;
            //    currentAlbum.Language = model.Language;
            //    currentAlbum.HasAdult = model.HasAdult;
            //    await UpdateAsync(currentManga);
            //    // MangaGenre
            //    var mangaGenres = new UpdateMangaGenreDto()
            //    {
            //        ListGenreId = model.ListGenreId,
            //        MangaId = currentManga.Id,
            //    };
            //    await _mangaGenreRepository.UpdateMangaGenre(mangaGenres);
            //    return JsonUtil.Success(currentManga);
            //}
            //catch (Exception ex)
            //{
            //    return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            //}
            throw new NotImplementedException();
        }
    }
}
