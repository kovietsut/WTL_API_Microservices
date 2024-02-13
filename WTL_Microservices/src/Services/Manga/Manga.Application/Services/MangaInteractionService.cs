using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Manga.Application.Services.Interfaces;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using ServiceStack.Redis;
using Shared.Common.Interfaces;
using Shared.DTOs;

namespace Manga.Application.Services
{
    public class MangaInteractionService : RepositoryBase<MangaInteraction, long, MangaContext>, IMangaInteractionService
    {
        private readonly ErrorCode _errorCodes;
        private readonly IBaseAuthService _baseAuthService;
        private readonly IRedisCacheRepository _iRedisCacheRepository;

        public MangaInteractionService(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork, IOptions<ErrorCode> errorCode,
            IBaseAuthService baseAuthService, IRedisCacheRepository iRedisCacheRepository) :
            base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
            _baseAuthService = baseAuthService;
            _iRedisCacheRepository = iRedisCacheRepository;
        }

        public async Task StoreMangaInteractionToDB()
        {
            try
            {
                // Get list Manga Interactions
                RedisEndpoint config = new RedisEndpoint { Host = "127.0.0.1", Port = 6379 };
                RedisClient redisClient = new RedisClient(config);
                var keys = redisClient.SearchKeys($"/api/interaction-manga*");
                if (keys.Any())
                {
                    List<MangaInteraction> list = redisClient.GetAll<MangaInteraction>(keys).Values.OrderBy(item => item.Id).ToList();
                    await CreateListAsync(list);
                }
                //// MangaInteraction
                //MangaInteraction mangaInteraction = new()
                //{
                //    IsEnabled = true,
                //    UserId = _baseAuthService.GetCurrentUserId(),
                //    MangaId = model.MangaId,
                //    ChapterId = model.ChapterId,
                //};
                //await CreateAsync(mangaInteraction);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
