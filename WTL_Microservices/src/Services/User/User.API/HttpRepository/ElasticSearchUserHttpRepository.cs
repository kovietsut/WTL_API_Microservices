using Newtonsoft.Json;
using User.API.ElasticSearch;
using User.API.HttpRepository.Interfaces;
using ILogger = Serilog.ILogger;

namespace User.API.HttpRepository
{
    public class ElasticSearchUserHttpRepository : IElasticSearchUserHttpRepository
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public ElasticSearchUserHttpRepository(HttpClient client, ILogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<List<UserSearchResult>> GetElasticSearchUser(int? pageNumber, int? pageSize)
        {
            try
            {
                //_client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var elasticSearchResult = await _client.GetAsync($"userElasticSearch/get-list?pageNumber={pageNumber}&pageSize={pageSize}");
                elasticSearchResult.EnsureSuccessStatusCode();
                var response = await elasticSearchResult.Content.ReadAsStringAsync();
                // Convert string to object
                var result = JsonConvert.DeserializeObject<List<UserSearchResult>>(response);
                if (result == null || result.Count == 0) return null;
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error($"Something went wrong: {ex.Message}");
                return null;
            }
        }
    }
}