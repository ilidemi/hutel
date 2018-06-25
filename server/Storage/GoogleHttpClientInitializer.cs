using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Http;

namespace hutel.Storage
{
    public class GoogleHttpClientInitializer : IConfigurableHttpClientInitializer, IHttpExecuteInterceptor, IHttpUnsuccessfulResponseHandler, IDisposable
    {
        private const string _envGoogleClientId = "GOOGLE_CLIENT_ID";
        private const string _envGoogleClientSecret = "GOOGLE_CLIENT_SECRET";
        private readonly GoogleTokenClient _tokenClient;
        private readonly GoogleAuthorizationCodeFlow _flow;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _userId;

        public GoogleHttpClientInitializer(string userId)
        {
            _tokenClient = new GoogleTokenClient();
            _clientId = Environment.GetEnvironmentVariable(_envGoogleClientId);
            _clientSecret = Environment.GetEnvironmentVariable(_envGoogleClientSecret);
            var clientSecrets = new ClientSecrets
            {
                ClientId = _clientId,
                ClientSecret = _clientSecret
            };
            _flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = clientSecrets
                }
            );
            _userId = userId;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._flow.Dispose();
            }
        }

        public void Initialize(ConfigurableHttpClient httpClient)
        {
            httpClient.MessageHandler.AddExecuteInterceptor(this);
            httpClient.MessageHandler.AddUnsuccessfulResponseHandler(this);
        }

        public async Task InterceptAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _tokenClient.GetAsync(_userId);
            request.Headers.Authorization =
                new AuthenticationHeaderValue(token.TokenType, token.AccessToken);
        }

        public async Task<bool> HandleResponseAsync(HandleUnsuccessfulResponseArgs args)
        {
            var content = await args.Response.Content.ReadAsStringAsync();
            if (args.Response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var oldToken = await _tokenClient.GetAsync(_userId);
                var newToken = await _flow.RefreshTokenAsync(
                    _userId, oldToken.RefreshToken, CancellationToken.None);
                await _tokenClient.StoreAsync(_userId, newToken);
                args.Request.Headers.Authorization =
                    new AuthenticationHeaderValue(newToken.TokenType, newToken.AccessToken);
                return true;
            }
            return false;
        }
    }
}