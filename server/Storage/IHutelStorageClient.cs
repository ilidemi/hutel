using System;
using System.Threading.Tasks;

namespace hutel.Storage
{
    public interface IHutelStorageClient
    {
        Task<string> ReadPointsAsStringAsync();

        Task WritePointsAsStringAsync(string data);

        Task<string> ReadTagsAsStringAsync();

        Task WriteTagsAsStringAsync(string data);

        Task<string> ReadChartsAsStringAsync();

        Task WriteChartsAsStringAsync(string data);
    }
}