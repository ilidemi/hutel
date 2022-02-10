using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace hutel.Storage
{
    public class GoogleDriveWritebackHutelStorageClient : IHutelStorageClient, IDisposable
    {
        private static readonly TimeSpan FlushPeriod = TimeSpan.FromSeconds(10);
        private string _settingsCache;
        private string _chartsCache;
        private string _pointsCache;
        private string _tagsCache;
        private bool _chartsChanged = false;
        private bool _pointsChanged = false;
        private bool _tagsChanged = false;
        private bool _settingsChanged = false;
        private SemaphoreSlim _chartsCacheLock = new SemaphoreSlim(1);
        private SemaphoreSlim _pointsCacheLock = new SemaphoreSlim(1);
        private SemaphoreSlim _tagsCacheLock = new SemaphoreSlim(1);
        private SemaphoreSlim _settingsCacheLock = new SemaphoreSlim(1);
        private GoogleDriveHutelStorageClient _googleDriveClient;
        private ILogger<GoogleDriveWritebackHutelStorageClient> _logger;

        public GoogleDriveWritebackHutelStorageClient(string userId, ILoggerFactory loggerFactory)
        {
            _googleDriveClient = new GoogleDriveHutelStorageClient(userId, loggerFactory);
            _logger = loggerFactory.CreateLogger<GoogleDriveWritebackHutelStorageClient>();
            Task.Run(() => FlushLoop());
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (disposing)
            {
                this._settingsCacheLock.Dispose();
                this._chartsCacheLock.Dispose();
                this._pointsCacheLock.Dispose();
                this._tagsCacheLock.Dispose();
                this._googleDriveClient.Dispose();
            }
        }

        private async Task FlushLoop()
        {
            while (true)
            {
                if (_settingsChanged)
                {
                    await _settingsCacheLock.WaitAsync();
                    var settings = _settingsCache;
                    _settingsChanged = false;
                    _settingsCacheLock.Release();
                    
                    try
                    {
                        await _googleDriveClient.WriteSettingsAsStringAsync(settings);
                        _logger.LogInformation("Settings writeback OK");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Settings write failed");
                    }
                }

                if (_chartsChanged)
                {
                    await _chartsCacheLock.WaitAsync();
                    var charts = _chartsCache;
                    _chartsChanged = false;
                    _chartsCacheLock.Release();
                    
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

                if (_pointsChanged)
                {
                    await _pointsCacheLock.WaitAsync();
                    var points = _pointsCache;
                    _pointsChanged = false;
                    _pointsCacheLock.Release();
                    
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

                if (_tagsChanged)
                {
                    await _tagsCacheLock.WaitAsync();
                    var tags = _tagsCache;
                    _tagsChanged = false;
                    _tagsCacheLock.Release();

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

        public async Task<string> ReadSettingsAsStringAsync()
        {
            if (_settingsCache == null)
            {
                await _settingsCacheLock.WaitAsync();
                try
                {
                    if (_settingsCache == null)
                    {
                        _settingsCache = await _googleDriveClient.ReadSettingsAsStringAsync();
                    }
                }
                finally
                {
                    _settingsCacheLock.Release();
                }
            }

            return _settingsCache;
        }

        public async Task<string> ReadChartsAsStringAsync()
        {
            if (_chartsCache == null)
            {
                await _chartsCacheLock.WaitAsync();
                try
                {
                    if (_chartsCache == null)
                    {
                        _chartsCache = await _googleDriveClient.ReadChartsAsStringAsync();
                    }
                }
                finally
                {
                    _chartsCacheLock.Release();
                }
            }

            return _chartsCache;
        }

        public async Task<string> ReadPointsAsStringAsync()
        {
            if (_pointsCache == null)
            {
                await _pointsCacheLock.WaitAsync();
                try
                {
                    if (_pointsCache == null)
                    {
                        _pointsCache = await _googleDriveClient.ReadPointsAsStringAsync();
                    }
                }
                finally
                {
                    _pointsCacheLock.Release();
                }
            }
            
            return _pointsCache;
        }

        public async Task<string> ReadTagsAsStringAsync()
        {
            if (_tagsCache == null)
            {
                await _tagsCacheLock.WaitAsync();
                try
                {
                    if (_tagsCache == null)
                    {
                        _tagsCache = await _googleDriveClient.ReadTagsAsStringAsync();
                    }
                }
                finally
                {
                    _tagsCacheLock.Release();
                }
            }
            
            return _tagsCache;
        }

        public async Task WriteSettingsAsStringAsync(string data)
        {
            await _settingsCacheLock.WaitAsync();
            _settingsCache = data;
            _settingsChanged = true;
            _settingsCacheLock.Release();
        }

        public async Task WriteChartsAsStringAsync(string data)
        {
            await _chartsCacheLock.WaitAsync();
            _chartsCache = data;
            _chartsChanged = true;
            _chartsCacheLock.Release();
        }

        public async Task WritePointsAsStringAsync(string data)
        {
            await _pointsCacheLock.WaitAsync();
            _pointsCache = data;
            _pointsChanged = true;
            _pointsCacheLock.Release();
        }

        public async Task WriteTagsAsStringAsync(string data)
        {
            await _tagsCacheLock.WaitAsync();
            _tagsCache = data;
            _tagsChanged =true;
            _tagsCacheLock.Release();
        }

        public async Task Reload()
        {
            await _settingsCacheLock.WaitAsync();
            try
            {
                _settingsCache = await _googleDriveClient.ReadSettingsAsStringAsync();
                _settingsChanged = false;
            }
            finally
            {
                _settingsCacheLock.Release();
            }

            await _chartsCacheLock.WaitAsync();
            try
            {
                _chartsCache = await _googleDriveClient.ReadChartsAsStringAsync();
                _chartsChanged = false;
            }
            finally
            {
                _chartsCacheLock.Release();
            }
            
            await _pointsCacheLock.WaitAsync();
            try
            {
                _pointsCache = await _googleDriveClient.ReadPointsAsStringAsync();
                _pointsChanged = false;
            }
            finally
            {
                _pointsCacheLock.Release();
            }
            
            await _tagsCacheLock.WaitAsync();
            try
            {
                _tagsCache = await _googleDriveClient.ReadTagsAsStringAsync();
                _tagsChanged = false;
            }
            finally
            {
                _tagsCacheLock.Release();
            }
        }
    }
}