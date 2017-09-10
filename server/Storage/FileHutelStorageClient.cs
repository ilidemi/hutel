using System;
using System.Threading.Tasks;

namespace hutel.Storage
{
    public class FileHutelStorageClient : IHutelStorageClient
    {
        private const string PointsFileName = "storage.json";
        private const string PointsBackupFileName = "storage.json.bak";
        private const string TagsFileName = "tags.json";
        private const string TagsBackupFileName = "tags.json.bak";
        private const string ChartsFileName = "charts.json";
        private const string ChartsBackupFileName = "charts.json.bak";
        private readonly IFileStorageClient _fileStorageClient;
        
        public FileHutelStorageClient(IFileStorageClient fileStorageClient)
        {
            _fileStorageClient = fileStorageClient;
        }

        public async Task<string> ReadPointsAsStringAsync()
        {
            return await ReadFileAsync(PointsFileName);
        }

        public async Task WritePointsAsStringAsync(string data)
        {
            await WriteFileAsync(PointsFileName, PointsBackupFileName, data);
        }

        public async Task<string> ReadTagsAsStringAsync()
        {
            return await ReadFileAsync(TagsFileName);
        }

        public async Task WriteTagsAsStringAsync(string data)
        {
            await WriteFileAsync(TagsFileName, TagsBackupFileName, data);
        }

        public async Task<string> ReadChartsAsStringAsync()
        {
            return await ReadFileAsync(ChartsFileName);
        }

        public async Task WriteChartsAsStringAsync(string data)
        {
            await WriteFileAsync(ChartsFileName, ChartsBackupFileName, data);
        }

        private async Task<string> ReadFileAsync(string fileName)
        {
            if (await _fileStorageClient.ExistsAsync(fileName))
            {
                return await _fileStorageClient.ReadAllAsync(fileName);
            }
            else
            {
                return string.Empty;
            }
        }

        private async Task WriteFileAsync(
            string fileName, string backupFileName, string data)
        {
            if (await _fileStorageClient.ExistsAsync(backupFileName))
            {
                await _fileStorageClient.DeleteAsync(backupFileName);
            }
            if (await _fileStorageClient.ExistsAsync(fileName))
            {
                await _fileStorageClient.CopyAsync(fileName, backupFileName);
            }
            await _fileStorageClient.WriteAllAsync(fileName, data);
        }
    }
}