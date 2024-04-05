using System.Net.Http.Headers;
using User.API.HttpRepository.Interfaces;

namespace User.API.Interceptors
{
    public class LoginHandler : DelegatingHandler
    {
        private readonly IAuthenticateHttpRepository _authenticateApiRepository;
        public LoginHandler(IAuthenticateHttpRepository authenticateApiRepository)
        {
            _authenticateApiRepository = authenticateApiRepository;
        }

        //protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        //        CancellationToken cancellationToken)
        //{
        //    var token = await _authenticateApiRepository.AuthenticateAsync();
        //    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
        //    return await base.SendAsync(request, cancellationToken);
        //}
    }
}
