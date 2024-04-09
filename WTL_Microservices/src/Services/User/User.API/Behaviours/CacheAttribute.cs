using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using Shared.Configurations;
using Shared.DTOs.RedisCache;
using Shared.SeedWork;
using User.API.Repositories.Interfaces;

namespace User.API.Behaviours
{
    public class CacheAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _timeToLiveSeconds;

        public CacheAttribute(int timeToLiveSeconds = 1000)
        {
            _timeToLiveSeconds = timeToLiveSeconds;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            try
            {
                var cacheService = context.HttpContext.RequestServices.GetRequiredService<IRedisCacheRepository>();
                var cacheKey = Util.GenerateCacheKeyFromRequest(context.HttpContext.Request);
                var cacheResponse = await cacheService.GetCachedResponseAsync(cacheKey);
                if (!string.IsNullOrEmpty(cacheResponse))
                {
                    var response = JsonConvert.DeserializeObject<RootObject>(cacheResponse)!;
                    var data = response.Value.Data;
                    context.Result = JsonUtil.Success(data, dataCount: data.Count());
                    return;
                }
                var executedContent = await next();
                await cacheService.SetCachedResponseAsync(cacheKey, executedContent.Result);
            }
            catch (Exception ex) { throw ex; }
        }
    }
}
