using System.Threading.Tasks;

namespace hutel.Logic
{
    public interface IStorageClient
    {
        Task<string> ReadAllAsync(string path);

        Task WriteAllAsync(string path, string contents);

        Task<bool> ExistsAsync(string path);

        Task CopyAsync(string source, string dest);

        Task DeleteAsync(string path);
    }
}