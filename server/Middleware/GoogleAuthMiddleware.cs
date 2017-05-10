using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using hutel.Logic;

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
        private readonly TimeSpan _expirationTime = TimeSpan.FromDays(30);
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly RequestDelegate _next;
        private readonly ISessionClient _sessionClient;
    
        public GoogleAuthMiddleware(RequestDelegate next)
        {
            _next = next;
            _clientId = Environment.GetEnvironmentVariable(_envGoogleClientId);
            _clientSecret = Environment.GetEnvironmentVariable(_envGoogleClientSecret);
            _sessionClient = new GoogleSessionClient();
        }
    
        public async Task Invoke(HttpContext httpContext)
        {
            var requestProtocol = httpContext.Items["protocol"];
            var redirectUri = $"{requestProtocol}://{httpContext.Request.Host}/login";
            var authEndpoint = _authEndpointBase +
                $"?client_id={_clientId}" +
                $"&redirect_uri={redirectUri}" +
                $"&response_type=code" +
                $"&scope=https://www.googleapis.com/auth/userinfo.profile+https://www.googleapis.com/auth/drive" +
                $"&access_type=online";

            if (httpContext.Request.Path == "/login")
            {
                if (httpContext.Request.Query.ContainsKey("code"))
                {
                    string authCode = httpContext.Request.Query["code"];
                    var tokenEndpoint = _tokenEndpointBase + 
                        $"?code={authCode}" + 
                        $"&client_id={_clientId}" +
                        $"&client_secret={_clientSecret}" + 
                        $"&redirect_uri={redirectUri}" +
                        $"&grant_type=authorization_code";
                    var httpClient = new HttpClient();
                    var tokenResponse = await httpClient.PostAsync(tokenEndpoint, new StringContent(""));
                    var tokenBody = await tokenResponse.Content.ReadAsStringAsync();
                    var tokenJObject = JObject.Parse(tokenBody);
                    var accessToken = tokenJObject["access_token"]?.Value<string>();
                    if (accessToken == null)
                    {
                        httpContext.Response.Redirect(authEndpoint);
                        return;
                    }
                    var handler = new JwtSecurityTokenHandler();
                    var idToken = handler.ReadJwtToken(tokenJObject["id_token"].Value<string>());

                    var newSession = new Session
                    {
                        SessionId = Guid.NewGuid().ToString(),
                        UserId = idToken.Subject,
                        Expiration = DateTime.Now + _expirationTime
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
            if (session.Expiration < DateTime.UtcNow)
            {
                httpContext.Response.Redirect(authEndpoint + $"&login_hint={session.UserId}");
                return;
            }
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