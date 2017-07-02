using System;
using System.Threading.Tasks;
using Google.Cloud.Datastore.V1;

namespace hutel.Session
{
    public class GoogleSessionClient : ISessionClient
    {
        private const string _envGoogleProjectId = "GOOGLE_PROJECT_ID";
        private const string _entityKind = "session";

        private readonly string _googleProjectId;
        private readonly DatastoreDb _db;
        private readonly KeyFactory _keyFactory;

        public GoogleSessionClient()
        {
            _googleProjectId = Environment.GetEnvironmentVariable(_envGoogleProjectId);
            _db = DatastoreDb.Create(_googleProjectId);
            _keyFactory = _db.CreateKeyFactory(_entityKind);
        }

        public async Task<SessionInfo> LookupSessionAsync(string sessionId)
        {
            var key = _keyFactory.CreateKey(sessionId);
            using (var transaction = await _db.BeginTransactionAsync())
            {
                var entity = await transaction.LookupAsync(key);
                if (entity == null)
                {
                    return null;
                }
                var session = new SessionInfo
                {
                    SessionId = entity["SessionId"].StringValue,
                    UserId = entity["UserId"].StringValue,
                    Expiration = DateTime.FromBinary(entity["Expiration"].IntegerValue)
                };
                return session;
            }
        }

        public async Task SaveSessionAsync(SessionInfo session)
        {
            var key = _keyFactory.CreateKey(session.SessionId);
            var entity = new Entity
            {
                Key = key,
                ["SessionId"] = session.SessionId,
                ["UserId"] = session.UserId,
                ["Expiration"] = session.Expiration.ToBinary()
            };
            using (var transaction = await _db.BeginTransactionAsync())
            {
                transaction.Upsert(entity);
                await transaction.CommitAsync();
            }
        }

        public async Task DeleteSessionAsync(string sessionId)
        {
            var key = _keyFactory.CreateKey(sessionId);
            using (var transaction = await _db.BeginTransactionAsync())
            {
                transaction.Delete(key);
                await transaction.CommitAsync();
            }
        }
    }
}