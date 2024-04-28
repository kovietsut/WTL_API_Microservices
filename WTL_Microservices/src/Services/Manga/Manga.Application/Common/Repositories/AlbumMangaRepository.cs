using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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

        public AlbumMangaRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork, IOptions<ErrorCode> errorCode) : base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
        }

        public async Task<List<long>> GetListAlbumManga(long albumId)
        {
            var listAlbumManga = FindAll().Where(x => x.AlbumId == albumId).Select(item => item.Manga.Id).ToList();
            return listAlbumManga;
        }

        public async Task<IActionResult> RemoveFromAlbum(long albumMangaId)
        {
            try
            {
                var albumManga = FindAll().FirstOrDefault(x => x.Id == albumMangaId);
                if(albumManga == null || !albumManga.IsEnabled)
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
                return JsonUtil.Success(result);
            }
            catch (Exception ex) 
            {
                return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.SystemError, ex.Message);
            }
        }
    }
}
