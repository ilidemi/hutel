using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Google;
using Google.Apis.Requests;
using Google.Cloud.Storage.V1;

namespace hutel.Storage
{
    public class GoogleCloudFileStorageClient: IFileStorageClient
    {
        private const string _bucket = "hutel-storage";
        private StorageClient _googleStorageClient;

        public GoogleCloudFileStorageClient()
        {
            _googleStorageClient = StorageClient.Create();
        }

        public async Task<string> ReadAllAsync(string path)
        {
            var stream = new MemoryStream();
            await _googleStorageClient.DownloadObjectAsync(_bucket, path, stream);
            await stream.FlushAsync();
            stream.Seek(0, SeekOrigin.Begin);
            var streamReader = new StreamReader(stream);
            var result = await streamReader.ReadToEndAsync();
            return result;
        }

        public async Task WriteAllAsync(string path, string contents)
        {
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            await streamWriter.WriteAsync(contents);
            await streamWriter.FlushAsync();
            stream.Seek(0, SeekOrigin.Begin);
            await _googleStorageClient.UploadObjectAsync(_bucket, path, null, stream);
        }

        public async Task<bool> ExistsAsync(string path)
        {
            try
            {
                await _googleStorageClient.GetObjectAsync(_bucket, path);
                return true;
            }
            catch (GoogleApiException ex)
            {
                if (ex.HttpStatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }
                throw;
            }
        }

        public async Task CopyAsync(string source, string dest)
        {
            await _googleStorageClient.CopyObjectAsync(_bucket, source, _bucket, dest);
        }

        public async Task DeleteAsync(string path)
        {
            await _googleStorageClient.DeleteObjectAsync(_bucket, path);
        }
    }
}