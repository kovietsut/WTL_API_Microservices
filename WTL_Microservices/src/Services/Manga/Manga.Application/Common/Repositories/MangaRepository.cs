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
using Shared.DTOs;
using Shared.DTOs.Manga;
using Shared.DTOs.MangaGenre;
using Shared.SeedWork;
using MangaEntity = Manga.Infrastructure.Entities.Manga;

namespace Manga.Application.Common.Repositories
{
    public class MangaRepository: RepositoryBase<MangaEntity, long, MangaContext>, IMangaRepository
    {
        private readonly ErrorCode _errorCodes;
        private readonly IMangaGenreRepository _mangaGenreRepository;
        public MangaRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork, IMangaGenreRepository mangaGenreRepository, 
            IOptions<ErrorCode> errorCode):
            base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
            _mangaGenreRepository = mangaGenreRepository;
        }

        public Task<MangaEntity> GetMangaById(long mangaId) => FindByCondition(x => x.Id == mangaId).SingleOrDefaultAsync();

        public async Task<IActionResult> GetManga(long userId)
        {
            var manga = await GetMangaById(userId);
            if (manga == null)
            {
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Manga does not exist");
            }
            var mangaResult = new
            {
                manga.Id,
                manga.IsEnabled,
                manga.CreatedAt,
                manga.CreatedBy,
                manga.Name,
                manga.Preface,
                manga.Type,
                manga.Status,
                manga.AmountOfReadings,
                manga.CoverImage,
                manga.Language,
                manga.HasAdult
            };
            return JsonUtil.Success(mangaResult);
        }

        public async Task<IActionResult> GetListManga(int? pageNumber, int? pageSize, string? searchText)
        {
            try
            {
                pageNumber ??= 1; pageSize ??= 10;
                var list = FindAll().Where(x => x.IsEnabled == true && (searchText == null || x.Name.Contains(searchText.Trim())))
                    .Select(x => new
                    {
                        MangaId = x.Id,
                        x.IsEnabled,
                        x.CreatedAt,
                        x.CreatedBy,
                        x.Name,
                        x.Preface,
                        x.Type,
                        x.Status,
                        x.AmountOfReadings,
                        x.CoverImage,
                        x.Language,
                        x.HasAdult
                    });
                var listData = list.Skip(((int)pageNumber - 1) * (int)pageSize)
                    .Take((int)pageSize).OrderByDescending(x => x.MangaId).ToList();
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

        public async Task<IActionResult> CreateManga(CreateMangaDto model)
        {
            try
            {
                // Validator
                var validator = new CreateMangaValidator();
                var check = await validator.ValidateAsync(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                // Manga
                MangaEntity manga = new()
                {
                    IsEnabled = true,
                    CreatedBy = model.CreatedBy,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Type = model.Type != null ? model.Type.Trim() : model.Type,
                    Name = model.Name != null ? model.Name.Trim() : model.Name,
                    Preface = model.Preface.Trim(),
                    Status = "DangChoDuyet",
                    AmountOfReadings = 0,
                    CoverImage = model.CoverImage != null ? model.CoverImage.Trim() : model.CoverImage,
                    Language = model.Language,
                    HasAdult = model.HasAdult
                };
                await CreateAsync(manga);
                // MangaGenre
                var mangaGenres = new CreateMangaGenreDto()
                {
                    ListGenreId = model.ListGenreId,
                    MangaId = manga.Id,
                };
                await _mangaGenreRepository.CreateMangaGenre(mangaGenres);
                return JsonUtil.Success(manga);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }

        public async Task<IActionResult> UpdateManga(long mangaId, UpdateMangaDto model)
        {
            try
            {
                // Validator
                var validator = new UpdateMangaValidator();
                var check = await validator.ValidateAsync(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                // Manga
                var currentManga = await GetByIdAsync(mangaId);
                if (currentManga == null)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "User does not exist");
                }
                currentManga.ModifiedAt = DateTimeOffset.UtcNow;
                currentManga.ModifiedBy = model.CreatedBy;
                currentManga.Name = model.Name;
                currentManga.Preface = model.Preface;
                currentManga.Type = model.Type;
                currentManga.Status = model.Status;
                currentManga.AmountOfReadings = model.AmountOfReadings;
                currentManga.CoverImage = model.CoverImage;
                currentManga.Language = model.Language;
                currentManga.HasAdult = model.HasAdult;
                await UpdateAsync(currentManga);
                // MangaGenre
                var mangaGenres = new UpdateMangaGenreDto()
                {
                    ListGenreId = model.ListGenreId,
                    MangaId = currentManga.Id,
                };
                await _mangaGenreRepository.UpdateMangaGenre(mangaGenres);
                return JsonUtil.Success(currentManga);
            }
            catch(Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }
    }
}
