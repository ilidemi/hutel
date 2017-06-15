using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using hutel.Logic;
using hutel.Session;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using hutel.Storage;

namespace hutel.Middleware
{
    public class GoogleAuthMiddleware
    {
        private const string _sessionCookieKey = "SessionId";
        private const string _envGoogleClientId = "GOOGLE_CLIENT_ID";
        private const string _envGoogleClientSecret = "GOOGLE_CLIENT_SECRET";
        private const string _authEndpointBase = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string _tokenEndpointBase = "https://www.googleapis.com/oauth2/v4/token";
        private const string _userInfoEndpoint = "https://www.googleapis.com/oauth2/v1/userinfo";
        private readonly TimeSpan _expirationTime = TimeSpan.FromDays(7);
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly RequestDelegate _next;
        private readonly ISessionClient _sessionClient;
        private readonly GoogleTokenClient _tokenClient;
    
        public GoogleAuthMiddleware(RequestDelegate next)
        {
            _next = next;
            _clientId = Environment.GetEnvironmentVariable(_envGoogleClientId);
            _clientSecret = Environment.GetEnvironmentVariable(_envGoogleClientSecret);
            _sessionClient = new GoogleSessionClient();
            _tokenClient = new GoogleTokenClient();
        }
    
        public async Task Invoke(HttpContext httpContext)
        {
            var requestProtocol = httpContext.Items["protocol"];
            var redirectUri = $"{requestProtocol}://{httpContext.Request.Host}/login";
            var authEndpoint = _authEndpointBase +
                $"?client_id={_clientId}" +
                $"&redirect_uri={redirectUri}" +
                $"&response_type=code" +
                $"&scope=openid+profile+email+https://www.googleapis.com/auth/userinfo.profile+https://www.googleapis.com/auth/drive" +
                $"&access_type=offline" +
                $"&prompt=consent";

            if (httpContext.Request.Path == "/login")
            {
                if (httpContext.Request.Query.ContainsKey("code"))
                {
                    string authCode = httpContext.Request.Query["code"];
                    var clientSecrets = new ClientSecrets
                    {
                        ClientId = _clientId,
                        ClientSecret = _clientSecret
                    };
                    var flow = new GoogleAuthorizationCodeFlow(
                        new GoogleAuthorizationCodeFlow.Initializer
                        {
                            ClientSecrets = clientSecrets
                        }
                    );
                    var tokenResponse = await flow.ExchangeCodeForTokenAsync(
                        "", authCode, redirectUri, CancellationToken.None);
                    var handler = new JwtSecurityTokenHandler();
                    var idToken = handler.ReadJwtToken(tokenResponse.IdToken);
                    var userId = idToken.Subject;
                    await _tokenClient.StoreAsync(userId, tokenResponse);
                    var newSession = new SessionInfo
                    {
                        SessionId = Guid.NewGuid().ToString(),
                        UserId = userId,
                        Expiration = DateTime.UtcNow + _expirationTime
                    };
                    await _sessionClient.SaveSessionAsync(newSession);
                    httpContext.Response.Cookies.Append(_sessionCookieKey, newSession.SessionId);
                    httpContext.Response.Redirect("/");
                    return;
                }
                else
                {
                    httpContext.Response.Redirect(authEndpoint);
                    return;
                }
            }
            if (!httpContext.Request.Cookies.ContainsKey(_sessionCookieKey))
            {
                httpContext.Response.Redirect(authEndpoint);
                return;
            }
            var sessionId = httpContext.Request.Cookies[_sessionCookieKey];
            var session = await _sessionClient.LookupSessionAsync(sessionId);
            if (session == null)
            {
                httpContext.Response.Redirect(authEndpoint);
                return;
            }
            var authEndpointWithHint = authEndpoint + $"&login_hint={session.UserId}";
            if (session.Expiration < DateTime.UtcNow)
            {
                httpContext.Response.Redirect(authEndpointWithHint);
                return;
            }
            var token = await _tokenClient.GetAsync(session.UserId);
            if (token == null)
            {
                httpContext.Response.Redirect(authEndpointWithHint);
                return;
            }
            session.Expiration = DateTime.UtcNow + _expirationTime;
            await _sessionClient.SaveSessionAsync(session);
            httpContext.Items["UserId"] = session.UserId;
            await _next.Invoke(httpContext);
        }
    }
    
    public static class GoogleAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseGoogleAuthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GoogleAuthMiddleware>();
        }
    }
}