using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace hutel.Storage
{
    public class LocalStorageClient: IStorageClient
    {
        public async Task<string> ReadAllAsync(string path)
        {
            using (var stream = File.OpenText(path))
            {
                var result = await stream.ReadToEndAsync();
                return result;
            }
        }

        public async Task WriteAllAsync(string path, string contents)
        {
            using (var stream = File.Create(path))
            {
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(contents);
                }
            }
        }

        public Task<bool> ExistsAsync(string path)
        {
            return Task.FromResult(File.Exists(path));
        }

        public async Task CopyAsync(string source, string dest)
        {
            using (var sourceStream = File.Open(source, FileMode.Open))
            {
                using (var destStream = File.Create(dest))
                {
                    await sourceStream.CopyToAsync(destStream);
                }
            }
        }

        public Task DeleteAsync(string path)
        {
           File.Delete(path);
           return Task.Delay(0);
        }
    }
}