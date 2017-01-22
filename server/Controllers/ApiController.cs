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
        private const string _pointsKey = "points";
        private const string _tagsKey = "tags";
        private const string _storagePath = "storage.json";
        private const string _storageBackupPath = "storage.json.bak";
        private const string _tagsPath = "tags.json";

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
        }

        [HttpGet("/api/points")]
        public IActionResult GetAll(string startDate)
        {
            var points = _memoryCache.Get<Dictionary<Guid, Point>>(_pointsKey);
            var tags = _memoryCache.Get<Dictionary<string, Tag>>(_tagsKey);
            return Json(
                points.Values
                    .Where(point => startDate == null || point.Date >= new HutelDate(startDate))
                    .Select(p => p.ToJson(tags))
                    .ToList());
        }

        [HttpPut("/api/points")]
        [ValidateModelState]
        public IActionResult PutAll([FromBody]PointsStorageJson replacementPoints)
        {
            var points = _memoryCache.Get<Dictionary<Guid, Point>>(_pointsKey);
            var tags = _memoryCache.Get<Dictionary<string, Tag>>(_tagsKey);
            IEnumerable<Point> pointsList;
            try
            {
                pointsList = replacementPoints.Select(p => Point.FromJson(p, tags));
            }
            catch(PointValidationException ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
            points.Clear();
            foreach (var p in pointsList)
            {
                points.Add(p.Id, p);
            }
            WriteStorage(points, tags);
            return Json(points.Values.Select(p => p.ToJson(tags)).ToList());
        }

        [HttpPost("/api/points")]
        [ValidateModelState]
        public IActionResult PostOne([FromBody]PointJson pointJson)
        {
            var points = _memoryCache.Get<Dictionary<Guid, Point>>(_pointsKey);
            var tags = _memoryCache.Get<Dictionary<string, Tag>>(_tagsKey);
            var id = Guid.NewGuid();
            Point point;
            try
            {
                point = Point.FromJson(pointJson, id, tags);
            }
            catch(PointValidationException ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
            points[id] = point;
            WriteStorage(points, tags);
            return Json(point.ToJson(tags));
        }

        [HttpPut("/api/point/{id}")]
        [ValidateModelState]
        public IActionResult PutOne(Guid id, [FromBody]PointJson pointJson)
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
                point = Point.FromJson(pointJson, id, tags);
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
            return Json(point.ToJson(tags));
        }

        private static Dictionary<Guid, Point> ReadStorage(Dictionary<string, Tag> tags)
        {
            if (System.IO.File.Exists(_storagePath))
            {
                var pointsString = System.IO.File.ReadAllText(_storagePath);
                return JsonConvert
                    .DeserializeObject<PointsStorageJson>(pointsString)
                    .ToDictionary(
                        pointWithIdJson => pointWithIdJson.Id,
                        pointWithIdJson => Point.FromJson(pointWithIdJson, tags));
            }
            else
            {
                return new Dictionary<Guid, Point>();
            }
        }

        private static void WriteStorage(
            Dictionary<Guid, Point> points,
            Dictionary<string, Tag> tags)
        {
            if (System.IO.File.Exists(_storageBackupPath))
            {
                System.IO.File.Delete(_storageBackupPath);
            }
            if (System.IO.File.Exists(_storagePath))
            {
                System.IO.File.Copy(_storagePath, _storageBackupPath);
            }
            var pointsJson = JsonConvert.SerializeObject(
                points.Values.Select(p => p.ToJson(tags)).ToList(),
                Formatting.Indented);
            System.IO.File.WriteAllText(_storagePath, pointsJson);
        }

        private static Dictionary<string, Tag> ReadTags()
        {
            if (!System.IO.File.Exists(_tagsPath))
            {
                throw new System.IO.FileNotFoundException("Tags config doesn't exist");
            }
            var tagsString = System.IO.File.ReadAllText(_tagsPath);
            var tagsList = JsonConvert.DeserializeObject<List<Tag>>(tagsString);
            var duplicateTags = tagsList
                .Select(tag => tag.Id)
                .GroupBy(id => id)
                .Where(g => g.Count() > 1);
            if (duplicateTags.Any())
            {
                throw new InvalidOperationException(
                    $"Duplicate tag ids in config: {string.Join(", ", duplicateTags)}");
            }
            return tagsList.ToDictionary(tag => tag.Id, tag => tag);
        }
    }
}
