using User.API.ElasticSearch;

namespace User.API.HttpRepository.Interfaces
{
    public interface IElasticSearchUserHttpRepository
    {
        Task<List<UserSearchResult>> GetElasticSearchUser(int? pageNumber, int? pageSize);
    }
}
