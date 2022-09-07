using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using hutel.Session;
using hutel.Storage;

namespace hutel.Middleware
{
    public class GoogleAuthMiddleware
    {
        private const string _sessionCookieKey = "SessionId";
        private const string _envGoogleClientId = "GOOGLE_CLIENT_ID";
        private const string _envGoogleClientSecret = "GOOGLE_CLIENT_SECRET";
        private const string _authEndpointBase = "https://accounts.google.com/o/oauth2/v2/auth";
        private readonly TimeSpan _expirationTime = TimeSpan.FromDays(7);
        private readonly TimeSpan _expirationTimeThreshold = TimeSpan.FromDays(6);
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly ISessionClient _sessionClient;
        private readonly GoogleTokenClient _tokenClient;
    
        public GoogleAuthMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<GoogleAuthMiddleware>();
            _clientId = Environment.GetEnvironmentVariable(_envGoogleClientId);
            _clientSecret = Environment.GetEnvironmentVariable(_envGoogleClientSecret);
            _sessionClient = new GoogleSessionClient();
            _tokenClient = new GoogleTokenClient();
        }
    
        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext.Items["UserId"] != null)
            {
                await _next.Invoke(httpContext);
                return;
            }

            var redirectUri = $"https://{httpContext.Request.Host}/login";
            var authEndpoint = _authEndpointBase +
                $"?client_id={_clientId}" +
                $"&redirect_uri={redirectUri}" +
                $"&response_type=code" +
                $"&scope=openid+profile+email+https://www.googleapis.com/auth/userinfo.profile+https://www.googleapis.com/auth/drive" +
                $"&access_type=offline";
            var promptQuery = "&prompt=consent";

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
                    httpContext.Response.Cookies.Append(
                        _sessionCookieKey,
                        newSession.SessionId,
                        new CookieOptions
                        {
                            Expires = new DateTimeOffset(newSession.Expiration)
                        });
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
                _logger.LogInformation(
                    LoggingEvents.SessionExpired,
                    null,
                    "No session id in cookie, redirecting to auth");
                httpContext.Response.Redirect(authEndpoint + promptQuery);
                return;
            }
            var sessionId = httpContext.Request.Cookies[_sessionCookieKey];
            var session = await _sessionClient.LookupSessionAsync(sessionId);
            if (session == null)
            {
                _logger.LogInformation(
                    LoggingEvents.SessionExpired,
                    null,
                    "No session in datastore, redirecting to auth");
                httpContext.Response.Redirect(authEndpoint);
                return;
            }
            var authEndpointWithHint = authEndpoint + $"&login_hint={session.UserId}";
            if (session.Expiration < DateTime.UtcNow)
            {
                _logger.LogInformation(
                    LoggingEvents.SessionExpired,
                    null,
                    "Session expiration time: {0}, now: {1}, redirecting to auth",
                    session.Expiration,
                    DateTime.UtcNow);
                await _sessionClient.DeleteSessionAsync(session.SessionId);
                httpContext.Response.Redirect(authEndpointWithHint);
                return;
            }
            var token = await _tokenClient.GetAsync(session.UserId);
            if (token == null || token.RefreshToken == null)
            {
                _logger.LogInformation(
                    LoggingEvents.SessionExpired,
                    null,
                    "Token is {0}, RefreshToken is {1}, redirecting to auth",
                    token,
                    token?.RefreshToken);
                httpContext.Response.Redirect(authEndpointWithHint + promptQuery);
                return;
            }
            if (session.Expiration < DateTime.UtcNow + _expirationTimeThreshold)
            {
                _logger.LogInformation(
                    LoggingEvents.SessionExpired,
                    null,
                    "Session expiration time: {0}, threshold: {1}, renewing the expiration time without redirect",
                    session.Expiration,
                    DateTime.UtcNow + _expirationTimeThreshold);
                session.Expiration = DateTime.UtcNow + _expirationTime;
                await _sessionClient.SaveSessionAsync(session);
                httpContext.Response.Cookies.Append(
                    _sessionCookieKey,
                    session.SessionId,
                    new CookieOptions
                    {
                        Expires = new DateTimeOffset(session.Expiration)
                    });
            }
            httpContext.Items["UserId"] = session.UserId;
            await _next.Invoke(httpContext);
        }
    }

    public static class LoggingEvents
    {
        public const int SessionExpired = 1000;
    }
    
    public static class GoogleAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseGoogleAuthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GoogleAuthMiddleware>();
        }
    }
}