using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Net;

namespace IssueToken
{
    public class EmailTokenDto
    {
        public string Email { get; set; }
    }
    public class IssueTokenHandler : DelegatingHandler
    {
        private readonly HttpClient _httpClient;

        public IssueTokenHandler(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                var tokenModel = new EmailTokenDto
                {
                    Email = "issuer@gmail.com"
                };
                var result = await _httpClient.PostAsJsonAsync("issuetoken/issue-token", tokenModel);
                if (result.EnsureSuccessStatusCode().IsSuccessStatusCode)
                {
                    var token = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                return await base.SendAsync(request, cancellationToken);
            }
            catch (HttpRequestException ex)
                when (ex.InnerException is SocketException { SocketErrorCode: SocketError.ConnectionRefused })
            {
                var hostWithPort = request.RequestUri.IsDefaultPort
                    ? request.RequestUri.DnsSafeHost
                    : $"{request.RequestUri.DnsSafeHost}:{request.RequestUri.Port}";
            }
            return new HttpResponseMessage(HttpStatusCode.BadGateway)
            {
                RequestMessage = request
            };
        }
    }
}
