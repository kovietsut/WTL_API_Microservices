using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Common;
using Shared.Common.Interfaces;
using Shared.DTOs;
using Shared.DTOs.AlbumManga;
using Shared.SeedWork;
using System.Collections.Generic;
using static MassTransit.ValidationResultExtensions;
using MangaEntity = Manga.Infrastructure.Entities.Manga;

namespace Manga.Application.Common.Repositories
{
    public class AlbumMangaRepository : RepositoryBase<AlbumManga, long, MangaContext>, IAlbumMangaRepository
    {
        private readonly ErrorCode _errorCodes;
        private readonly ISasTokenGenerator _sasTokenGenerator;

        public AlbumMangaRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork, 
            IOptions<ErrorCode> errorCode, ISasTokenGenerator sasTokenGenerator) : base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
            _sasTokenGenerator = sasTokenGenerator;
        }

        public async Task<IActionResult> GetListAlbumManga(long albumId, int? pageNumber, int? pageSize)
        {
            try
            {
                pageNumber ??= 1; pageSize ??= 10;
                var listAlbumManga = FindAll().Where(x => x.AlbumId == albumId && x.IsEnabled == true)
                    .Select(x => new
                    {
                        AlbumId = x.Id,
                        x.IsEnabled,
                        x.Manga.Id,
                        x.Manga.Name,
                        x.Manga.CoverImage,
                        x.Manga.CreatedBy,
                    });
                var listData = listAlbumManga.Skip(((int)pageNumber - 1) * (int)pageSize)
                    .Take((int)pageSize).OrderByDescending(x => x.AlbumId).ToList();
                if (listAlbumManga != null)
                {
                    var totalRecords = listAlbumManga.Count();
                    return JsonUtil.Success(listData, dataCount: totalRecords);
                }
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Empty List Data");
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.SystemError, ex.Message);
            }
        }

        public async Task<IActionResult> RemoveFromAlbum(long albumMangaId)
        {
            try
            {
                var albumManga = FindAll().FirstOrDefault(x => x.Id == albumMangaId);
                if (albumManga == null || !albumManga.IsEnabled)
                {
                    return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.SystemError, "Manga not found");
                }
                albumManga.IsEnabled = false;
                await UpdateAsync(albumManga);
                return JsonUtil.Success("Manga has been removed!");
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.SystemError, ex.Message);
            }
        }

        public async Task<IActionResult> RemoveListFromAlbum(string albumMangaIds)
        {
            try
            {
                //await BeginTransactionAsync();
                var list = new List<AlbumManga>();
                if (albumMangaIds.IsNullOrEmpty())
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "ListAlbumMangaId cannot be null");
                }
                var listIds = Util.SplitStringToArray(albumMangaIds);
                var mangas = FindAll().Where(x => listIds.Contains(x.Id));
                if(mangas == null || mangas.Count() == 0)
                {
                    return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Cannot get list manga");
                }
                foreach (var manga in mangas)
                {
                    manga.IsEnabled = false;
                    list.Add(manga);
                }
                var listRemoved = mangas.Select(x => x.Id).ToList();
                if (list.Count != 0)
                {
                    await UpdateListAsync(list);
                }
                //await EndTransactionAsync();
                return JsonUtil.Success(listRemoved);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.SystemError, ex.Message);
            }
        }
        

        public async Task<IActionResult> SaveToAlbum(SaveToAlbumDto model)
        {
            try
            {
                var albumMangas = new List<AlbumManga>();
                var existedMangas = FindAll().Where(x => model.ListMangaId.Contains(x.MangaId));
                foreach (var manga in existedMangas)
                {
                    manga.IsEnabled = true;
                    model.ListMangaId.Remove(manga.MangaId);
                }
                await UpdateListAsync(existedMangas);
                if(model.ListMangaId.Count > 0)
                {
                    model.ListMangaId.ForEach(id =>
                    {
                        albumMangas.Add(new AlbumManga
                        {
                            IsEnabled = true,
                            AlbumId = model.AlbumId,
                            MangaId = id,
                            AddedDate = DateTimeOffset.UtcNow
                        });
                    });
                    var result = await CreateListAsync(albumMangas);
                    return JsonUtil.Success();
                }
                return JsonUtil.Success();
            }
            catch (Exception ex) 
            {
                return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.SystemError, ex.Message);
            }
        }
    }
}
