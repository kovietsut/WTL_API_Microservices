using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.DTOs;
using Shared.SeedWork;
using User.API.HttpRepository.Interfaces;

namespace User.API.HttpRepository
{
    public class ElasticSearchUserHttpRepository: IElasticSearchUserHttpRepository
    {
        private readonly HttpClient _client;
        private readonly ErrorCode _errorCodes;

        public ElasticSearchUserHttpRepository(HttpClient client, IOptions<ErrorCode> errorCodes)
        {
            _client = client;
            _errorCodes = errorCodes.Value;
        }

        public async Task<IActionResult> GetElasticSearchUser(int? pageNumber, int? pageSize)
        {
            try
            {
                _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJlbWFpbCI6Im5ndXllbnRpZW5waGF0OXhAZ21haWwuY29tIiwic3ViIjoiMSIsImp0aSI6IjZkMTRhNWRiLTI4M2YtNGI1ZC1iMjI2LThmNWE5MGFiZmZlOCIsIklkIjoiMSIsIkVtYWlsIjoibmd1eWVudGllbnBoYXQ5eEBnbWFpbC5jb20iLCJSb2xlIjoiMSIsIm5iZiI6MTcxMjMwMjY1MywiZXhwIjoxNzEyOTA3NDUzLCJpYXQiOjE3MTIzMDI2NTMsImlzcyI6Iklzc3VlciIsImF1ZCI6Iklzc3VlciJ9.E4QNVt9adKdsPhftMJYVeYcKtorIiivdK5bwpID65oXOd14D1kNLZTprzn2seQ0Bk7c8sNJq7w2IEQzvJIfXcQ");
                var elasticSearchResult = await _client.GetFromJsonAsync<dynamic>($"userElasticSearch/get-list?pageNumber={pageNumber}&pageSize={pageSize}");
                if (elasticSearchResult != null)
                {
                    return JsonUtil.Success(elasticSearchResult);
                }
                return JsonUtil.Error(StatusCodes.Status404NotFound, _errorCodes.Status404.NotFound, "Empty List Data");
            }
            catch (Exception ex)
            {
                return JsonUtil.Error(StatusCodes.Status401Unauthorized, _errorCodes.Status401.Unauthorized, ex.Message);
            }
        }
    }
}
