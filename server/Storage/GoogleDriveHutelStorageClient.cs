using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Drive.v2;
using hutel.Logic;
using Microsoft.Extensions.Caching.Memory;

using GoogleFile = Google.Apis.Drive.v2.Data.File;
using ParentReference = Google.Apis.Drive.v2.Data.ParentReference;

namespace hutel.Storage
{
    public class GoogleDriveHutelStorageClient : IHutelStorageClient
    {
        private const string ApplicationName = "Human Telemetry";
        private const string RootFolderName = "Hutel";
        private const string PointsFileBaseName = "storage";
        private const string PointsFileName = "storage.json";
        private const string TagsFileBaseName = "tags";
        private const string TagsFileName = "tags.json";
        private const string FolderMimeType = "application/vnd.google-apps.folder";
        private const string JsonMimeType = "application/octet-stream";
        private readonly string _userId;
        private bool _initialized;
        private DriveService _driveService;
        private string _rootFolderId;
        private string _pointsFileId;
        private DateTime? _pointsLastBackupDate;
        private string _tagsFileId;
        private DateTime? _tagsLastBackupDate;
        
        public GoogleDriveHutelStorageClient(string userId)
        {
            _userId = userId;
            _initialized = false;
        }

        public async Task<string> ReadPointsAsStringAsync()
        {
            await InitAsync();
            Console.WriteLine("ReadPointsAsStringAsync");
            return await ReadFileAsStringAsync(_pointsFileId);
        }

        public async Task WritePointsAsStringAsync(string data)
        {
            await InitAsync();
            Console.WriteLine("WritePointsAsStringAsync");
            await BackupStorageIfNeededAsync();
            await WriteFileAsStringAsync(_pointsFileId, data);
        }

        public async Task<string> ReadTagsAsStringAsync()
        {
            await InitAsync();
            Console.WriteLine("ReadTagsAsStringAsync");
            return await ReadFileAsStringAsync(_tagsFileId);
        }

        public async Task WriteTagsAsStringAsync(string data)
        {
            await InitAsync();
            Console.WriteLine("WriteTagsAsStringAsync");
            await BackupTagsIfNeededAsync();
            await WriteFileAsStringAsync(_tagsFileId, data);
        }

        private async Task InitAsync()
        {
            if (_initialized)
            {
                return;
            }
            var HttpClientInitializer = new GoogleHttpClientInitializer(_userId);
            _driveService = new DriveService(
                new DriveService.Initializer
                {
                    HttpClientInitializer = HttpClientInitializer,
                    ApplicationName = ApplicationName
                }
            );

            var rootFolder = await GetOrCreateFileAsync(RootFolderName, FolderMimeType, null);
            _rootFolderId = rootFolder.Id;
            var pointsFileTask = GetOrCreateFileAsync(PointsFileName, null, _rootFolderId);
            var tagsFileTask = GetOrCreateFileAsync(TagsFileName, null, _rootFolderId);
            var pointsLastBackupTask = FindLastBackupAsync(
                PointsFileBaseName, PointsFileName, _rootFolderId);
            var tagsLastBackupTask = FindLastBackupAsync(
                TagsFileBaseName, TagsFileName, _rootFolderId);

            var pointsFile = await pointsFileTask;
            var tagsFile = await tagsFileTask;
            var pointsLastBackup = await pointsLastBackupTask;
            var tagsLastBackup = await tagsLastBackupTask;

            _pointsFileId = pointsFile.Id;
            _tagsFileId = tagsFile.Id;
            _pointsLastBackupDate = pointsLastBackup?.CreatedDate;
            _tagsLastBackupDate = tagsLastBackup?.CreatedDate;
            _initialized = true;
        }

        private async Task<GoogleFile> FindLastBackupAsync(
            string baseName, string fullName, string parent)
        {
            var listRequest = _driveService.Files.List();
            listRequest.Q = $"title contains '{baseName}' and '{parent}' in parents";
            listRequest.Spaces = "drive";
            var fileList = await listRequest.ExecuteAsync();
            var validFiles = fileList.Items
                .Where(file => file.Labels.Trashed != true)
                .Where(file => file.Title != fullName)
                .ToList();
            validFiles.Sort((a, b) => DateTimeOptCompareDesc(a.CreatedDate, b.CreatedDate));
            if (validFiles.Count > 0)
            {
                return validFiles[0];
            }
            else
            {
                return null;
            }
        }

        private static int DateTimeOptCompareDesc(DateTime? a, DateTime? b)
        {
            if (!a.HasValue && !b.HasValue)
            {
                return 0;
            }
            else if (!a.HasValue)
            {
                return 1;
            }
            else if (!b.HasValue)
            {
                return -1;
            }
            else
            {
                return -DateTime.Compare(a.Value, b.Value);
            }
        }

        private async Task BackupStorageIfNeededAsync()
        {
            if (!_pointsLastBackupDate.HasValue ||
                (DateTime.UtcNow - _pointsLastBackupDate.Value).Days >= 1)
            {
                var backupDateString = DateTime.Now.ToString("yyyy-MM-dd");
                var backupFileName = $"{PointsFileBaseName}-{backupDateString}.json";
                await CopyFileAsync(_pointsFileId, backupFileName);
            }
        }

        private async Task BackupTagsIfNeededAsync()
        {
            if (!_tagsLastBackupDate.HasValue ||
                (DateTime.UtcNow - _tagsLastBackupDate.Value).Days >= 1)
            {
                var backupDateString = DateTime.Now.ToString("yyyy-MM-dd");
                var backupFileName = $"{TagsFileBaseName}-{backupDateString}.json";
                await CopyFileAsync(_tagsFileId, backupFileName);
            }
        }

        private async Task<GoogleFile> GetOrCreateFileAsync(
            string name, string mimeType, string parent)
        {
            var listRequest = _driveService.Files.List();
            var parentId = parent != null ? parent : "root";
            var mimeQuery = mimeType != null ? $"mimeType = '{mimeType}' and " : "";
            listRequest.Q = mimeQuery + $"title = '{name}' and '{parentId}' in parents";
            listRequest.Spaces = "drive";
            var fileList = await listRequest.ExecuteAsync();
            var validFiles = fileList.Items.Where(file => file.Labels.Trashed != true).ToList();
            if (validFiles.Count > 0)
            {
                return validFiles[0];
            }
            else
            {
                return await CreateFileAsync(name, mimeType, parent);
            }
        }

        private async Task<GoogleFile> CreateFileAsync(string name, string mimeType, string parent)
        {
            var parentId = parent != null ? parent : "root";
            var fileMetadata = new GoogleFile
            {
                Title = name,
                MimeType = mimeType,
                Parents = new List<ParentReference>
                    {
                        parent != null
                            ? new ParentReference{ Id = parentId }
                            : new ParentReference{ IsRoot = true }
                    }
            };
            var request = _driveService.Files.Insert(fileMetadata);
            request.Fields = "id";
            var file = await request.ExecuteAsync();
            return file;
        }

        private async Task<GoogleFile> CopyFileAsync(string fileId, string name)
        {
            var fileMetadata = new GoogleFile
            {
                Title = name
            };
            var copyRequest = _driveService.Files.Copy(fileMetadata, fileId);
            var file = await copyRequest.ExecuteAsync();
            return file;
        }

        private async Task<string> ReadFileAsStringAsync(string fileId)
        {
            Console.WriteLine("ReadFileAsStringAsync");
            var downloadRequest = _driveService.Files.Get(fileId);
            var stream = new MemoryStream();
            var progress = await downloadRequest.DownloadAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var streamReader = new StreamReader(stream);
            var contents = await streamReader.ReadToEndAsync();
            return contents;
        }

        private async Task WriteFileAsStringAsync(string fileId, string data)
        {
            Console.WriteLine("WriteFileAsStringAsync");
            
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            streamWriter.Write(data);
            streamWriter.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            var file = new GoogleFile();
            var updateRequest = _driveService.Files.Update(file, fileId, stream, JsonMimeType);
            var progress = await updateRequest.UploadAsync();
        }
    }
}