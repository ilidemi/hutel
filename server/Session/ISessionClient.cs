using System;
using System.Threading.Tasks;

namespace hutel.Session
{
    public interface ISessionClient
    {
        Task<SessionInfo> LookupSessionAsync(string sessionId);

        Task SaveSessionAsync(SessionInfo session);
    }

    public class SessionInfo
    {
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public DateTime Expiration { get; set; }
    }
}