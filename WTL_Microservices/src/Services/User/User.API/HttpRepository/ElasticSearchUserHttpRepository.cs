using Microsoft.Extensions.Options;
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
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6Im5ndXllbnRpZW5waGF0OXhAZ21haWwuY29tIiwic3ViIjoiMSIsImp0aSI6IjZkMTRhNWRiLTI4M2YtNGI1ZC1iMjI2LThmNWE5MGFiZmZlOCIsIklkIjoiMSIsIkVtYWlsIjoibmd1eWVudGllbnBoYXQ5eEBnbWFpbC5jb20iLCJSb2xlIjoiMSIsIm5iZiI6MTcxMjMwMjY1MywiZXhwIjoxNzEyOTA3NDUzLCJpYXQiOjE3MTIzMDI2NTMsImlzcyI6Iklzc3VlciIsImF1ZCI6Iklzc3VlciJ9.E4QNVt9adKdsPhftMJYVeYcKtorIiivdK5bwpID65oXOd14D1kNLZTprzn2seQ0Bk7c8sNJq7w2IEQzvJIfXcQ");
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