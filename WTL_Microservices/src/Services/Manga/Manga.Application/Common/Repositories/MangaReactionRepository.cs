using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.Common.Interfaces;
using Shared.DTOs;
using Shared.DTOs.MangaChapterReaction;
using Shared.SeedWork;
using static Shared.DTOs.MangaChapterReaction.MangaInteractionDto;

namespace Manga.Application.Common.Repositories
{
    public class MangaReactionRepository : RepositoryBase<MangaInteraction, long, MangaContext>, IMangaReactionRepository
    {
        private readonly ErrorCode _errorCodes;
        private readonly IBaseAuthService _baseAuthService;
        private readonly IRedisCacheRepository _iRedisCacheRepository;

        public MangaReactionRepository(MangaContext dbContext, IUnitOfWork<MangaContext> unitOfWork, IOptions<ErrorCode> errorCode, 
            IBaseAuthService baseAuthService, IRedisCacheRepository iRedisCacheRepository) :
            base(dbContext, unitOfWork)
        {
            _errorCodes = errorCode.Value;
            _baseAuthService = baseAuthService;
            _iRedisCacheRepository = iRedisCacheRepository;
        }

        public async Task<List<MangaInteraction>> GetListMangaFollowing(long mangaId)
        {
            var listMangaReactions = FindAll().Where(x => x.MangaId == mangaId && x.InteractionType.Equals("Favorite")).ToList();
            return listMangaReactions;
        }

        public async Task<long> GetListMangaReaction(long mangaId)
        {
            var listMangaReactions = FindAll().Where(x => x.MangaId == mangaId && x.InteractionType.Equals("Favorite")).ToList();
            return listMangaReactions.Count;
        }

        private string GetInteractionKey(MangaInteractionDto model)
        {
            if (model.ChapterId == null)
            {
                if (model.InteractionType.Equals("Favorite"))
                {
                    return $"/api/interaction-manga-favorite/{model.MangaId}";
                }
                else if (model.InteractionType.Equals("Follow"))
                {
                    return $"/api/interaction-manga-follow/{model.MangaId}";
                }
                else if (model.InteractionType.Equals("Like"))
                {
                    return $"/api/interaction-manga-like/{model.MangaId}";
                }
            }
            else
            {
                if (model.InteractionType.Equals("Like"))
                {
                    return $"/api/interaction-chapter-like/{model.MangaId}/{model.ChapterId}";
                }
                else if (model.InteractionType.Equals("Follow"))
                {
                    return $"/api/interaction-chapter-follow/{model.MangaId}/{model.ChapterId}";
                }
            }

            // Default key for unknown interaction type or null chapter id
            return $"/api/interaction-chapter-favorite/{model.MangaId}/{model.ChapterId}";
        }

        public async Task<IActionResult> CreateMangaInteraction(MangaInteractionDto model)
        {
            try
            {
                // Validator
                var validator = new MangaInteractionValidator();
                var check = await validator.ValidateAsync(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                // MangaInteraction
                MangaInteraction mangaInteraction = new()
                {
                    IsEnabled = true,
                    UserId = _baseAuthService.GetCurrentUserId(),
                    MangaId = model.MangaId,
                    ChapterId = model.ChapterId,
                    InteractionType = model.InteractionType,
                };
                // Set Key Based On Favorite || Like || Follow
                var key = "";
                if(model.ChapterId == null && model.InteractionType.Equals("Favorite"))
                {
                    key = $"/api/interaction-manga-favorite/{model.MangaId}";
                }
                else if(model.ChapterId == null && model.InteractionType.Equals("Follow"))
                {
                    key = $"/api/interaction-manga-follow/{model.MangaId}";
                }
                else if (model.ChapterId == null && model.InteractionType.Equals("Like"))
                {
                    key = $"/api/interaction-manga-like/{model.MangaId}";
                }
                else if (model.ChapterId != null && model.InteractionType.Equals("Like"))
                {
                    key = $"/api/interaction-chapter-like/{model.MangaId}/{model.ChapterId}";
                }
                else if (model.ChapterId != null && model.InteractionType.Equals("Follow"))
                {
                    key = $"/api/interaction-chapter-follow/{model.MangaId}/{model.ChapterId}";
                }
                else
                {
                    key = $"/api/interaction-chapter-favorite/{model.MangaId}/{model.ChapterId}";
                }
                // Check if key exists and remove it
                if (await _iRedisCacheRepository.GetCachedResponseAsync(key) != null)
                {
                    return JsonUtil.Error(StatusCodes.Status409Conflict, _errorCodes.Status409.Conflict, "Manga or chapter is stored");
                }
                // Store to cache
                await _iRedisCacheRepository.SetCachedResponseAsync(key, mangaInteraction);
                return JsonUtil.Success(mangaInteraction);
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }

        public async Task<IActionResult> RemoveMangaInteraction(MangaInteractionDto model)
        {
            try
            {
                // Validator
                var validator = new MangaInteractionValidator();
                var check = await validator.ValidateAsync(model);
                if (!check.IsValid)
                {
                    return JsonUtil.Errors(StatusCodes.Status400BadRequest, _errorCodes.Status400.ConstraintViolation, check.Errors);
                }
                // Check if key exists and remove it
                var key = GetInteractionKey(model);
                if (await _iRedisCacheRepository.GetCachedResponseAsync(key) != null)
                {
                    await _iRedisCacheRepository.RemoveCached(key);
                    return JsonUtil.Success("Remove Manga Or Chapter Success");
                }
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Manga or Chapter not found");
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.APIServerError, ex.Message);
            }
        }
    }
}
