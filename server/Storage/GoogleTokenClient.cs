using System;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using Google.Cloud.Datastore.V1;
using Newtonsoft.Json;

namespace hutel.Storage
{
    public class GoogleTokenClient
    {
        private const string _envGoogleProjectId = "GOOGLE_PROJECT_ID";
        private const string _entityKind = "token";
        private readonly string _googleProjectId;
        private readonly DatastoreDb _db;
        private readonly KeyFactory _keyFactory;

        public GoogleTokenClient()
        {
            _googleProjectId = Environment.GetEnvironmentVariable(_envGoogleProjectId);
            _db = DatastoreDb.Create(_googleProjectId);
            _keyFactory = _db.CreateKeyFactory(_entityKind);
        }
        
        public async Task<TokenResponse> GetAsync(string userId)
        {
            var key = _keyFactory.CreateKey(userId);
            using (var transaction = await _db.BeginTransactionAsync())
            {
                var entity = await transaction.LookupAsync(key);
                if (entity == null)
                {
                    return null;
                }
                var result = JsonConvert.DeserializeObject<TokenResponse>(entity["Token"].StringValue);
                return result;
            }
        }

        public async Task StoreAsync(string userId, TokenResponse value)
        {
            var key = _keyFactory.CreateKey(userId);
            var entity = new Entity
            {
                Key = key,
                ["Token"] = new Value
                {
                    StringValue = JsonConvert.SerializeObject(value),
                    ExcludeFromIndexes = true
                }
            };
            using (var transaction = await _db.BeginTransactionAsync())
            {
                transaction.Upsert(entity);
                await transaction.CommitAsync();
            }
        }
    }
}