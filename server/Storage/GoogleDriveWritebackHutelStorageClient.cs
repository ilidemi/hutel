using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace hutel.Storage
{
    public class GoogleDriveWritebackHutelStorageClient : IHutelStorageClient
    {
        private static readonly TimeSpan FlushPeriod = TimeSpan.FromSeconds(10);
        private string _chartsPending;
        private string _pointsPending;
        private string _tagsPending;
        private SemaphoreSlim _chartsPendingLock = new SemaphoreSlim(1);
        private SemaphoreSlim _pointsPendingLock = new SemaphoreSlim(1);
        private SemaphoreSlim _tagsPendingLock = new SemaphoreSlim(1);
        private GoogleDriveHutelStorageClient _googleDriveClient;
        private ILogger<GoogleDriveWritebackHutelStorageClient> _logger;

        public GoogleDriveWritebackHutelStorageClient(string userId)
        {
            _googleDriveClient = new GoogleDriveHutelStorageClient(userId);
            _logger = Program.LoggerFactory.CreateLogger<GoogleDriveWritebackHutelStorageClient>();
            Task.Run(() => FlushLoop());
        }

        private async Task FlushLoop()
        {
            while (true)
            {
                await _chartsPendingLock.WaitAsync();
                var charts = _chartsPending;
                _chartsPending = null;
                _chartsPendingLock.Release();
                if (charts != null)
                {
                    try
                    {
                        await _googleDriveClient.WriteChartsAsStringAsync(charts);
                        _logger.LogInformation("Charts writeback OK");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Charts write failed");
                    }
                }
                
                await _pointsPendingLock.WaitAsync();
                var points = _pointsPending;
                _pointsPending = null;
                _pointsPendingLock.Release();
                if (points != null)
                {
                    try
                    {
                        await _googleDriveClient.WritePointsAsStringAsync(points);
                        _logger.LogInformation("Points writeback OK");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Points write failed");
                    }
                }
                
                await _tagsPendingLock.WaitAsync();
                var tags = _tagsPending;
                _tagsPending = null;
                _tagsPendingLock.Release();
                if (tags != null)
                {
                    try
                    {
                        await _googleDriveClient.WriteTagsAsStringAsync(tags);
                        _logger.LogInformation("Tags writeback OK");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Tags write failed");
                    }
                }

                await Task.Delay(FlushPeriod);
            }
        }

        public async Task<string> ReadChartsAsStringAsync()
        {
            return await _googleDriveClient.ReadChartsAsStringAsync();
        }

        public async Task<string> ReadPointsAsStringAsync()
        {
            return await _googleDriveClient.ReadPointsAsStringAsync();
        }

        public async Task<string> ReadTagsAsStringAsync()
        {
            return await _googleDriveClient.ReadTagsAsStringAsync();
        }

        public async Task WriteChartsAsStringAsync(string data)
        {
            await _chartsPendingLock.WaitAsync();
            _chartsPending = data;
            _chartsPendingLock.Release();
        }

        public async Task WritePointsAsStringAsync(string data)
        {
            await _pointsPendingLock.WaitAsync();
            _pointsPending = data;
            _pointsPendingLock.Release();
        }

        public async Task WriteTagsAsStringAsync(string data)
        {
            await _tagsPendingLock.WaitAsync();
            _tagsPending = data;
            _tagsPendingLock.Release();
        }
    }
}