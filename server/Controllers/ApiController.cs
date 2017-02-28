using System;
using System.Collections.Generic;
using System.Linq;
using hutel.Filters;
using hutel.Logic;
using hutel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace hutel.Controllers
{
    public class ApiController : Controller
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;
        private readonly IStorageClient _storageClient;
        private const string _pointsKey = "points";
        private const string _tagsKey = "tags";
        private const string _storagePath = "storage.json";
        private const string _storageBackupPath = "storage.json.bak";
        private const string _tagsPath = "tags.json";
        private const string _tagsBackupPath = "tags.json.bak";

        public ApiController(IMemoryCache memoryCache, ILogger<ApiController> logger)
        {
            _memoryCache = memoryCache;
            Dictionary<string, Tag> tags;
            if (!memoryCache.TryGetValue(_tagsKey, out tags))
            {
                tags = ReadTags();
                _memoryCache.Set(
                    _tagsKey,
                    tags,
                    new MemoryCacheEntryOptions()
                        .SetPriority(CacheItemPriority.NeverRemove)
                );
            }
            Dictionary<Guid, Point> points;
            if (!_memoryCache.TryGetValue(_pointsKey, out points))
            {
                points = ReadStorage(tags);
                _memoryCache.Set(
                    _pointsKey,
                    points,
                    new MemoryCacheEntryOptions()
                        .SetPriority(CacheItemPriority.NeverRemove)
                );
            }
            _logger = logger;
            _storageClient = new LocalStorageClient();
        }

        [HttpGet("/api/points")]
        public IActionResult GetAllPoints(string startDate)
        {
            var points = _memoryCache.Get<Dictionary<Guid, Point>>(_pointsKey);
            var tags = _memoryCache.Get<Dictionary<string, Tag>>(_tagsKey);
            var filteredPoints = points.Values
                    .Where(point => startDate == null || point.Date >= new HutelDate(startDate))
                    .ToList();
            filteredPoints.Sort((p1, p2) => p2.Date.DateTime.CompareTo(p1.Date.DateTime));
            return Json(filteredPoints.Select(p => p.ToDataContract(tags)));
        }

        [HttpPut("/api/points")]
        [ValidateModelState]
        public IActionResult PutAllPoints([FromBody]PointsStorageDataContract replacementPoints)
        {
            var points = _memoryCache.Get<Dictionary<Guid, Point>>(_pointsKey);
            var tags = _memoryCache.Get<Dictionary<string, Tag>>(_tagsKey);
            IEnumerable<Point> pointsList;
            try
            {
                pointsList = replacementPoints.Select(p => Point.FromDataContract(p, tags));
            }
            catch(PointValidationException ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
            var duplicatePoints = pointsList
                .Select(point => point.Id)
                .GroupBy(id => id)
                .Where(g => g.Count() > 1);
            if (duplicatePoints.Any())
            {
                return new BadRequestObjectResult(
                    new InvalidOperationException(
                        $"Duplicate point ids: {string.Join(", ", duplicatePoints)}").ToString());
            }
            points.Clear();
            foreach (var p in pointsList)
            {
                points.Add(p.Id, p);
            }
            WriteStorage(points, tags);
            return Json(points.Values.Select(p => p.ToDataContract(tags)).ToList());
        }

        [HttpPost("/api/points")]
        [ValidateModelState]
        public IActionResult PostOnePoint([FromBody]PointDataContract input)
        {
            var points = _memoryCache.Get<Dictionary<Guid, Point>>(_pointsKey);
            var tags = _memoryCache.Get<Dictionary<string, Tag>>(_tagsKey);
            var id = Guid.NewGuid();
            Point point;
            try
            {
                point = Point.FromDataContract(input, id, tags);
            }
            catch(PointValidationException ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
            points[id] = point;
            WriteStorage(points, tags);
            return Json(point.ToDataContract(tags));
        }

        [HttpPut("/api/point/{id}")]
        [ValidateModelState]
        public IActionResult PutOnePoint(Guid id, [FromBody]PointDataContract input)
        {
            var points = _memoryCache.Get<Dictionary<Guid, Point>>(_pointsKey);
            var tags = _memoryCache.Get<Dictionary<string, Tag>>(_tagsKey);
            if (!points.ContainsKey(id))
            {
                return new BadRequestObjectResult(
                    new ArgumentOutOfRangeException($"No point with id {id}").ToString());
            }
            Point point;
            try
            {
                point = Point.FromDataContract(input, id, tags);
            }
            catch(PointValidationException ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
            if (string.Compare(point.TagId, points[id].TagId, true) != 0) {
                return new BadRequestObjectResult(
                    new ArgumentOutOfRangeException(
                        $"Tag id differs from the known one. Expected: {points[id].TagId}, got: {point.TagId}").ToString());
            }
            points[id] = point;
            WriteStorage(points, tags);
            return Json(point.ToDataContract(tags));
        }

        [HttpGet("/api/tags")]
        public IActionResult GetAllTags()
        {
            var tags = _memoryCache.Get<Dictionary<string, Tag>>(_tagsKey);
            var tagsDataContract = tags.Values.Select(tag => tag.ToDataContract());
            return Json(tagsDataContract);
        }

        [HttpPut("/api/tags")]
        [ValidateModelState]
        public IActionResult PutAllTags([FromBody]List<Tag> replacementTagsList)
        {
            if (!replacementTagsList.Any())
            {
                return new BadRequestObjectResult(
                    new InvalidOperationException("Tags list is empty"));
            }
            var points = _memoryCache.Get<Dictionary<Guid, Point>>(_pointsKey);
            var tags = _memoryCache.Get<Dictionary<string, Tag>>(_tagsKey);
            var duplicateTags = replacementTagsList
                .Select(tag => tag.Id)
                .GroupBy(id => id)
                .Where(g => g.Count() > 1);
            if (duplicateTags.Any())
            {
                return new BadRequestObjectResult(
                    new InvalidOperationException(
                        $"Duplicate tag ids: {string.Join(", ", duplicateTags)}").ToString());
            }
            var pointsDataContract = points.Values.Select(point => point.ToDataContract(tags));
            var replacementTags = replacementTagsList.ToDictionary(tag => tag.Id, tag => tag);
            try
            {
                foreach (var pointDataContract in pointsDataContract)
                {
                    Point.FromDataContract(pointDataContract, replacementTags);
                }
            }
            catch(PointValidationException ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
            tags.Clear();
            foreach (var tag in replacementTagsList)
            {
                tags.Add(tag.Id, tag);
            }
            WriteTags(tags);
            return Json(tags.Values);
        }

        private Dictionary<Guid, Point> ReadStorage(Dictionary<string, Tag> tags)
        {
            if (_storageClient.Exists(_storagePath))
            {
                var pointsString = _storageClient.ReadAll(_storagePath);
                var pointsDataContractList =
                    JsonConvert.DeserializeObject<PointsStorageDataContract>(pointsString);
                var duplicatePoints = pointsDataContractList
                    .GroupBy(point => point.Id)
                    .Where(g => g.Count() > 1);
                if (duplicatePoints.Any())
                {
                    throw new InvalidOperationException(
                        $"Duplicate point ids in config: {string.Join(", ", duplicatePoints)}");
                }
                return pointsDataContractList.ToDictionary(
                    point => point.Id,
                    point => Point.FromDataContract(point, tags));
            }
            else
            {
                return new Dictionary<Guid, Point>();
            }
        }

        private void WriteStorage(
            Dictionary<Guid, Point> points,
            Dictionary<string, Tag> tags)
        {
            if (_storageClient.Exists(_storageBackupPath))
            {
                _storageClient.Delete(_storageBackupPath);
            }
            if (_storageClient.Exists(_storagePath))
            {
                _storageClient.Copy(_storagePath, _storageBackupPath);
            }
            var pointsJson = JsonConvert.SerializeObject(
                points.Values.Select(p => p.ToDataContract(tags)).ToList(),
                Formatting.Indented);
            _storageClient.WriteAll(_storagePath, pointsJson);
        }

        private Dictionary<string, Tag> ReadTags()
        {
            if (!_storageClient.Exists(_tagsPath))
            {
                throw new InvalidOperationException("Tags config doesn't exist");
            }
            var tagsString = _storageClient.ReadAll(_tagsPath);
            var tagsDataContractList =
                JsonConvert.DeserializeObject<List<TagDataContract>>(tagsString);
            if (!tagsDataContractList.Any())
            {
                throw new InvalidOperationException("Tags list is empty");
            }
            var duplicateTags = tagsDataContractList
                .GroupBy(tag => tag.Id)
                .Where(g => g.Count() > 1);
            if (duplicateTags.Any())
            {
                throw new InvalidOperationException(
                    $"Duplicate tag ids in config: {string.Join(", ", duplicateTags)}");
            }
            return tagsDataContractList.ToDictionary(
                tag => tag.Id,
                tag => Tag.FromDataContract(tag));
        }

        private void WriteTags(Dictionary<string, Tag> tags)
        {
            if (_storageClient.Exists(_tagsBackupPath))
            {
                _storageClient.Delete(_tagsBackupPath);
            }
            if (_storageClient.Exists(_tagsPath))
            {
                _storageClient.Copy(_tagsPath, _tagsBackupPath);
            }
            var tagsDataContract = tags.Values.Select(tag => tag.ToDataContract());
            var tagsJson = JsonConvert.SerializeObject(tagsDataContract, Formatting.Indented);
            _storageClient.WriteAll(_tagsPath, tagsJson);
        }
    }
}
