using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Shared.DTOs;
using Shared.SeedWork;
using MangaEntity = Manga.Infrastructure.Entities.Manga;

namespace Manga.Application.Common.Repositories
{
    public class MangaRepository : RepositoryBase<MangaEntity, long, MangaContext>, IMangaRepository
    {
        private readonly ErrorCode _errorCodes;
        public MangaRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork, IOptions<ErrorCode> errorCode):
            base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
        }

        public Task<MangaEntity> GetMangaById(long mangaId) => FindByCondition(x => x.Id == mangaId).SingleOrDefaultAsync();

        public async Task<IActionResult> GetManga(long userId)
        {
            var manga = await GetMangaById(userId);
            if (manga == null)
            {
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Manga does not exist");
            }
            return JsonUtil.Success(new
            {
                manga.Id, manga.IsEnabled, manga.CreatedAt, manga.CreatedBy, manga.Name, manga.Preface, manga.Type, manga.Status, 
                manga.AmountOfReadings, manga.CoverImage, manga.Language, manga.HasAdult
            });
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
    }
}
