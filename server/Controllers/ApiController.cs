using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hutel.Filters;
using hutel.Logic;
using hutel.Models;
using hutel.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace hutel.Controllers
{
    public class ApiController : Controller
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger _logger;
        private readonly Lazy<IHutelStorageClient> _storageClientLazy;
        private IHutelStorageClient _storageClient
        {
            get { return _storageClientLazy.Value; }
        }
        private const string _envUseGoogleStorage = "HUTEL_USE_GOOGLE_STORAGE";
        private const string _envUseGoogleDrive = "HUTEL_USE_GOOGLE_DRIVE";        
        private const string _pointsKey = "points";
        private const string _tagsKey = "tags";
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        { 
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public ApiController(IMemoryCache memoryCache, ILogger<ApiController> logger)
        {
            if (Environment.GetEnvironmentVariable(_envUseGoogleDrive) == "1")
            {
                _storageClientLazy = new Lazy<IHutelStorageClient>(() =>
                {
                    var userId = (string)HttpContext.Items["UserId"];
                    return new GoogleDriveHutelStorageClient(userId);
                });
            }
            else if (Environment.GetEnvironmentVariable(_envUseGoogleStorage) == "1")
            {
                _storageClientLazy = new Lazy<IHutelStorageClient>(() => 
                    new FileHutelStorageClient(new GoogleCloudFileStorageClient()));
            }
            else
            {
                _storageClientLazy = new Lazy<IHutelStorageClient>(() =>
                    new FileHutelStorageClient(new LocalFileStorageClient()));
            }
            _logger = logger;
            _memoryCache = memoryCache;
        }

        [HttpGet("/api/points")]
        public async Task<IActionResult> GetAllPoints(string startDate)
        {
            var tags = await ReadTags();
            var points = await ReadStorage(tags);
            var filteredPoints = points.Values
                    .Where(point => startDate == null || point.Date >= new HutelDate(startDate))
                    .ToList();
            filteredPoints.Sort((p1, p2) => 
            {
                var result = p2.Date.DateTime.CompareTo(p1.Date.DateTime);
                if (result == 0)
                {
                    result = p2.SubmitTimestamp.DateTime.CompareTo(p1.SubmitTimestamp.DateTime);
                }
                return result;
            });
            return Json(filteredPoints.Select(p => p.ToDataContract(tags)));
        }

        [HttpPut("/api/points")]
        [ValidateModelState]
        public async Task<IActionResult> PutAllPoints(
            [FromBody]PointsStorageDataContract replacementPoints)
        {
            var tags = await ReadTags();
            var points = await ReadStorage(tags);
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
            await WriteStorage(points, tags);
            return Json(points.Values.Select(p => p.ToDataContract(tags)).ToList());
        }

        [HttpPost("/api/points")]
        [ValidateModelState]
        public async Task<IActionResult> PostOnePoint([FromBody]PointDataContract input)
        {
            var tags = await ReadTags();
            var points = await ReadStorage(tags);
            var id = Guid.NewGuid();
            Point point;
            try
            {
                point = Point.FromDataContract(
                    input, id, new HutelTimestamp(DateTime.UtcNow), tags);
            }
            catch(PointValidationException ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
            points[id] = point;
            await WriteStorage(points, tags);
            return Json(point.ToDataContract(tags));
        }

        [HttpPut("/api/point/{id}")]
        [ValidateModelState]
        public async Task<IActionResult> PutOnePoint(Guid id, [FromBody]PointDataContract input)
        {
            var tags = await ReadTags();
            var points = await ReadStorage(tags);
            if (!points.ContainsKey(id))
            {
                return new BadRequestObjectResult(
                    new ArgumentOutOfRangeException($"No point with id {id}").ToString());
            }
            Point point;
            try
            {
                point = Point.FromDataContract(input, id, points[id].SubmitTimestamp, tags);
            }
            catch(PointValidationException ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
            if (string.Compare(point.TagId, points[id].TagId, true) != 0) {
                return new BadRequestObjectResult(
                    new ArgumentOutOfRangeException(
                        $"Tag id differs from the known one. " +
                        $"Expected: {points[id].TagId}, got: {point.TagId}").ToString());
            }
            points[id] = point;
            await WriteStorage(points, tags);
            return Json(point.ToDataContract(tags));
        }

        [HttpGet("/api/tags")]
        public async Task<IActionResult> GetAllTags()
        {
            var tags = await ReadTags();
            var tagsDataContract = tags.Values.Select(tag => tag.ToDataContract());
            return Json(tagsDataContract);
        }

        [HttpPut("/api/tags")]
        [ValidateModelState]
        public async Task<IActionResult> PutAllTags([FromBody]List<TagDataContract> inputList)
        {
            if (!inputList.Any())
            {
                return new BadRequestObjectResult(
                    new InvalidOperationException("Tags list is empty"));
            }
            var tags = await ReadTags();
            var points = await ReadStorage(tags);
            var replacementTagsList = inputList.Select(input => Tag.FromDataContract(input));
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
            await WriteTags(tags);
            return Json(tags.Values);
        }
        
        [HttpGet("/api/charts")]
        public async Task<IActionResult> GetAllCharts()
        {
            var chartsString = await _storageClient.ReadChartsAsStringAsync();
            return Content(chartsString, "application/json");
        }

        [HttpPut("/api/charts")]
        public async Task<IActionResult> PutAllCharts([FromBody] string chartsString)
        {
            await _storageClient.WriteChartsAsStringAsync(chartsString);
            return Ok();
        }

        private async Task<Dictionary<Guid, Point>> ReadStorage(Dictionary<string, Tag> tags)
        {
            Dictionary<Guid, Point> points;
            if (_memoryCache.TryGetValue(_pointsKey, out points))
            {
                return points;
            }
            var pointsString = await _storageClient.ReadPointsAsStringAsync();
            var pointsDataContractList = pointsString == string.Empty
                ? new PointsStorageDataContract()
                : JsonConvert.DeserializeObject<PointsStorageDataContract>(pointsString);
            var duplicatePoints = pointsDataContractList
                .GroupBy(point => point.Id)
                .Where(g => g.Count() > 1);
            if (duplicatePoints.Any())
            {
                throw new InvalidOperationException(
                    $"Duplicate point ids in config: {string.Join(", ", duplicatePoints)}");
            }
            points = pointsDataContractList.ToDictionary(
                point => point.Id,
                point => Point.FromDataContract(point, tags));

            _memoryCache.Set(_pointsKey, points);
            return points;
        }

        private async Task WriteStorage(
            Dictionary<Guid, Point> points,
            Dictionary<string, Tag> tags)
        {
            var pointsJson = JsonConvert.SerializeObject(
                points.Values.Select(p => p.ToDataContract(tags)).ToList(), jsonSettings);
            await _storageClient.WritePointsAsStringAsync(pointsJson);
            _memoryCache.Set(_pointsKey, points);
        }

        private async Task<Dictionary<string, Tag>> ReadTags()
        {
            Dictionary<string, Tag> tags;
            if (_memoryCache.TryGetValue(_tagsKey, out tags))
            {
                return tags;
            }
            var tagsString = await _storageClient.ReadTagsAsStringAsync();
            var tagsDataContractList =
                JsonConvert.DeserializeObject<List<TagDataContract>>(tagsString);
            if (tagsString == string.Empty || !tagsDataContractList.Any())
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
            tags = tagsDataContractList.ToDictionary(
                tag => tag.Id,
                tag => Tag.FromDataContract(tag));
            _memoryCache.Set(_tagsKey, tags);
            return tags;
        }

        private async Task WriteTags(Dictionary<string, Tag> tags)
        {
            var tagsDataContract = tags.Values.Select(tag => tag.ToDataContract());
            var tagsJson = JsonConvert.SerializeObject(tagsDataContract, jsonSettings);
            await _storageClient.WriteTagsAsStringAsync(tagsJson);
            _memoryCache.Set(_tagsKey, tags);
        }
    }
}
