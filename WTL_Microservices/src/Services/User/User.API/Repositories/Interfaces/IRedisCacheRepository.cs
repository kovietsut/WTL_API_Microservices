namespace User.API.Repositories.Interfaces
{
    public interface IRedisCacheRepository
    {
        Task<string> GetCachedResponseAsync(string cacheKey);
        Task SetCachedResponseAsync(string cacheKey, object response);
        Task RemoveCached(string cacheKey);
    }
}
