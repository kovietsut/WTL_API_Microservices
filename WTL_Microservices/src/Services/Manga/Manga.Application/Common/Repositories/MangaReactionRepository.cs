using Contracts.Domains.Interfaces;
using Infrastructure.Common.Repositories;
using Manga.Application.Common.Repositories.Interfaces;
using Manga.Infrastructure.Entities;
using Manga.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
                var key = "";
                if(model.ChapterId == null)
                {
                    key = $"/api/interaction-manga/{mangaInteraction.Id}/mangaId:{model.MangaId}";
                }
                else
                {
                    key = $"/api/interaction-chapter/{mangaInteraction.Id}/mangaId:{model.MangaId}/chapterId:{model.ChapterId}";
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
    }
}
