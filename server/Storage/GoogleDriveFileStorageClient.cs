using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v2;
using GoogleFile = Google.Apis.Drive.v2.Data.File;
using ParentReference = Google.Apis.Drive.v2.Data.ParentReference;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;

namespace hutel.Storage
{
    public class GoogleDriveFileStorageClient: IFileStorageClient
    {
        private const string _applicationName = "Human Telemetry";
        private const string _rootFolderName = "Hutel";
        private const string _folderMimeType = "application/vnd.google-apps.folder";
        private bool _initialized = false;
        private readonly string _userId;
        private DriveService _driveService;
        private string _rootFolderId;

        public GoogleDriveFileStorageClient(string userId)
        {
            _userId = userId;
        }

        public async Task<string> ReadAllAsync(string path)
        {
            if (!_initialized)
            {
                await Init();
            }

            var file = await FindFileInRootFolder(path);
            if (file == null)
            {
                throw new InvalidOperationException($"File {path} doesn't exist");
            }
            
            var downloadRequest = _driveService.Files.Get(file.Id);
            var stream = new MemoryStream();
            var progress = await downloadRequest.DownloadAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var streamReader = new StreamReader(stream);
            var contents = await streamReader.ReadToEndAsync();
            return contents;
        }

        public async Task WriteAllAsync(string path, string contents)
        {
            if (!_initialized)
            {
                await Init();
            }

            var existingFile = await FindFileInRootFolder(path);
            if (existingFile != null)
            {
                var deleteRequest = _driveService.Files.Delete(existingFile.Id);
                await deleteRequest.ExecuteAsync();
            }

            var newFileMetadata = new GoogleFile
            {
                Title = path,
                Parents = new List<ParentReference>
                {
                    new ParentReference { Id = _rootFolderId }
                }
            };
            var stream = new MemoryStream();
            var streamWriter = new StreamWriter(stream);
            streamWriter.Write(contents);
            streamWriter.Flush();
            stream.Seek(0, SeekOrigin.Begin);
            var streamReader = new StreamReader(stream);
            var data = await streamReader.ReadToEndAsync();
            stream.Seek(0, SeekOrigin.Begin);
            var uploadRequest = _driveService.Files.Insert(
                newFileMetadata, stream, "application/octet-stream");
            var progress = await uploadRequest.UploadAsync();
        }

        public async Task<bool> ExistsAsync(string path)
        {
            if (!_initialized)
            {
                await Init();
            }

            var file = await FindFileInRootFolder(path);
            return file != null;
        }

        public async Task CopyAsync(string source, string dest)
        {
            if (!_initialized)
            {
                await Init();
            }

            var sourceFile = await FindFileInRootFolder(source);
            if (sourceFile == null)
            {
                throw new InvalidOperationException($"File {source} doesn't exist");
            }

            var destFile = new GoogleFile
            {
                Title = dest
            };

            var copyRequest = _driveService.Files.Copy(destFile, sourceFile.Id);
            await copyRequest.ExecuteAsync();
        }

        public async Task DeleteAsync(string path)
        {
            if (!_initialized)
            {
                await Init();
            }

            var file = await FindFileInRootFolder(path);
            if (file == null)
            {
                return;
            }

            var deleteRequest = _driveService.Files.Delete(file.Id);
            await deleteRequest.ExecuteAsync();
        }

        private async Task Init()
        {
            var HttpClientInitializer = new GoogleHttpClientInitializer(_userId);
            _driveService = new DriveService(
                new DriveService.Initializer
                {
                    HttpClientInitializer = HttpClientInitializer,
                    ApplicationName = _applicationName
                }
            );

            _rootFolderId = (await CreateOrGetRootFolder()).Id;
        }

        private async Task<GoogleFile> CreateOrGetRootFolder()
        {
            var listRequest = _driveService.Files.List();
            listRequest.Q = $"mimeType = '{_folderMimeType}' and title = '{_rootFolderName}' and 'root' in parents";
            listRequest.Spaces = "drive";
            var fileList = await listRequest.ExecuteAsync();
            if (fileList.Items.Count > 0)
            {
                return fileList.Items[0];
            }
            else
            {
                var folderMetadata = new GoogleFile
                {
                    Title = _rootFolderName,
                    MimeType = _folderMimeType,
                    Parents = new List<ParentReference>
                        {
                            new ParentReference{ IsRoot = true }
                        }
                };
                var request = _driveService.Files.Insert(folderMetadata);
                request.Fields = "id";
                return await request.ExecuteAsync();
            }
        }

        private async Task<GoogleFile> FindFileInRootFolder(string path)
        {
            var listRequest = _driveService.Files.List();
            listRequest.Q = $"title = '{path}' and '{_rootFolderId}' in parents";
            listRequest.Spaces = "drive";
            var fileList = await listRequest.ExecuteAsync();
            if (fileList.Items.Count == 0)
            {
                return null;
            }
            return fileList.Items[0];
        }
    }
}