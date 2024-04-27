using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.DTOs;
using Shared.DTOs.AlbumManga;
using Shared.SeedWork;
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

        public async Task<IActionResult> RemoveFromAlbum(long mangaId)
        {
            try
            {
                var mangaInAlbum = FindAll().FirstOrDefault(x => x.MangaId == mangaId);
                if(mangaInAlbum == null || !mangaInAlbum.IsEnabled)
                {
                    return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.SystemError, "Manga not found");
                }
                mangaInAlbum.IsEnabled = false;
                await UpdateAsync(mangaInAlbum);
                return JsonUtil.Success("Manga has been removed!");
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
