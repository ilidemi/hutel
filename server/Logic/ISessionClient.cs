using System;
using System.Threading.Tasks;

namespace hutel.Logic
{
    public interface ISessionClient
    {
        Task<Session> LookupSessionAsync(string sessionId);

        Task SaveSessionAsync(Session session);
    }

    public class Session
    {
        public string SessionId { get; set; }
        public string UserId { get; set; }
        public DateTime Expiration { get; set; }
    }
}