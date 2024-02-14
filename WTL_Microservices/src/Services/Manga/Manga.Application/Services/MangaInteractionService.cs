using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Manga.Application.Services.Interfaces;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServiceStack.Redis;
using Shared.Common.Interfaces;
using Shared.DTOs;
using Shared.DTOs.MangaChapterReaction;
using Shared.DTOs.RedisCache;
using Shared.SeedWork;

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

        private string GetInteractionKey(long mangaId, long? chapterId, string interactionType)
        {
            if (chapterId == null)
            {
                if (interactionType.Equals("Favorite"))
                {
                    return $"/api/interaction-manga-favorite/{mangaId}";
                }
                else if (interactionType.Equals("Follow"))
                {
                    return $"/api/interaction-manga-follow/{mangaId}";
                }
                else if (interactionType.Equals("Like"))
                {
                    return $"/api/interaction-manga-like/{mangaId}";
                }
            }
            else
            {
                if (interactionType.Equals("Like"))
                {
                    return $"/api/interaction-chapter-like/{mangaId}/{chapterId}";
                }
                else if (interactionType.Equals("Follow"))
                {
                    return $"/api/interaction-chapter-follow/{mangaId}/{chapterId}";
                }
            }
            // Default key for unknown interaction type or null chapter id
            return $"/api/interaction-chapter-favorite/{mangaId}/{chapterId}";
        }


        public async Task<bool> CheckMangaChapterInteraction(long mangaId, long? chapterId, string interactionType)
        {
            try
            {
                var key = GetInteractionKey(mangaId, chapterId, interactionType);
                var cacheResponse = await _iRedisCacheRepository.GetCachedResponseAsync(key);
                if (cacheResponse == null) return false;
                var response = JsonConvert.DeserializeObject<MangaInteraction>(cacheResponse)!;
                if (chapterId == null && interactionType.Equals("Favorite"))
                {
                    var isLikedManga = response.MangaId == mangaId && response.InteractionType.Equals("Favorite");
                    //var isLikedManga = Any(x => x.InteractionType.Equals("Favorite") && x.MangaId == mangaId);
                    return isLikedManga;
                }
                else if (chapterId == null && interactionType.Equals("Follow"))
                {
                    var isFollowedManga = response.MangaId == mangaId && response.InteractionType.Equals("Follow");
                    //var isFollowedManga = Any(x => x.InteractionType.Equals("Follow") && x.MangaId == mangaId);
                    return isFollowedManga;
                }
                else if (chapterId == null && interactionType.Equals("Like"))
                {
                    var isLikedManga = response.MangaId == mangaId && response.InteractionType.Equals("Like");
                    //var isLikedManga = Any(x => x.InteractionType.Equals("Like") && x.MangaId == mangaId);
                    return isLikedManga;
                }
                else if (chapterId != null && interactionType.Equals("Favorite"))
                {
                    var isLikedChapter = response.MangaId == mangaId && response.ChapterId == chapterId && response.InteractionType.Equals("Favorite");
                    //var isLikedManga = Any(x => x.InteractionType.Equals("Favorite") && x.MangaId == mangaId && x.ChapterId == chapterId);
                    return isLikedChapter;
                }
                else if (chapterId != null && interactionType.Equals("Follow"))
                {
                    var isFollowedChapter = response.MangaId == mangaId && response.ChapterId == chapterId && response.InteractionType.Equals("Follow");
                    //var isFollowedChapter = Any(x => x.InteractionType.Equals("Follow") && x.MangaId == mangaId && x.ChapterId == chapterId);
                    return isFollowedChapter;
                }
                else
                {
                    var isLikedChapter = response.MangaId == mangaId && response.ChapterId == chapterId && response.InteractionType.Equals("Like");
                    //var isLikedChapter = Any(x => x.InteractionType.Equals("Like") && x.MangaId == mangaId && x.ChapterId == chapterId);
                    return isLikedChapter;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task StoreMangaFavoriteToDB()
        {
            try
            {
                // Get list Manga Interactions
                var list = new List<MangaInteraction>();
                RedisEndpoint config = new RedisEndpoint { Host = "127.0.0.1", Port = 6380 };
                RedisClient redisClient = new RedisClient(config);
                var keys = redisClient.SearchKeys($"/api/interaction-manga-favorite*");
                if (keys.Any())
                {
                    // List MangaInteractions
                    foreach (var key in keys)
                    {
                        var cache = await _iRedisCacheRepository.GetCachedResponseAsync(key);
                        if (!string.IsNullOrEmpty(cache))
                        {
                            var mangaInteraction = JsonConvert.DeserializeObject<MangaInteraction>(cache)!;
                            list.Add(mangaInteraction);
                        }
                    }
                    await CreateListAsync(list);
                    // Clear cache
                    foreach (var removedKey in keys)
                    {
                        await _iRedisCacheRepository.RemoveCached(removedKey);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
