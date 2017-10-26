using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using hutel.Filters;
using hutel.Logic;
using hutel.Models;
using hutel.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace hutel.Controllers
{
    public class ApiController : Controller
    {
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
        private static Dictionary<string, string> tagsCache = new Dictionary<string, string>();
        private static SemaphoreSlim tagsCacheLock = new SemaphoreSlim(1);
        private static Dictionary<string, string> chartsCache = new Dictionary<string, string>();
        private static SemaphoreSlim chartsCacheLock = new SemaphoreSlim(1);
        private static Dictionary<string, string> pointsCache = new Dictionary<string, string>();
        private static SemaphoreSlim pointsCacheLock = new SemaphoreSlim(1);

        public ApiController(ILogger<ApiController> logger)
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
        }

        [HttpGet("/api/points")]
        public async Task<IActionResult> GetAllPoints(string startDate, string tagId, string tagIds)
        {
            await tagsCacheLock.WaitAsync();
            var tags = await ReadTags();
            tagsCacheLock.Release();

            await pointsCacheLock.WaitAsync();
            var points = await ReadStorage(tags);
            pointsCacheLock.Release();

            var tagIdsList = tagIds != null
                ? tagIds.Split(',')
                : tagId != null
                    ? new string[] { tagId }
                    : tags.Keys.ToArray();

            var filteredPoints = points.Values
                    .Where(point => startDate == null || point.Date >= new HutelDate(startDate))
                    .Where(point => tagIdsList.Contains(point.TagId))
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
            await tagsCacheLock.WaitAsync();
            var tags = await ReadTags();
            tagsCacheLock.Release();

            await pointsCacheLock.WaitAsync();
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
            pointsCacheLock.Release();
            return Json(points.Values.Select(p => p.ToDataContract(tags)).ToList());
        }

        [HttpPost("/api/points")]
        [ValidateModelState]
        public async Task<IActionResult> PostOnePoint([FromBody]PointDataContract input)
        {
            await tagsCacheLock.WaitAsync();
            var tags = await ReadTags();
            tagsCacheLock.Release();

            await pointsCacheLock.WaitAsync();
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
            pointsCacheLock.Release();
            return Json(point.ToDataContract(tags));
        }

        [HttpPut("/api/point/{id}")]
        [ValidateModelState]
        public async Task<IActionResult> PutOnePoint(Guid id, [FromBody]PointDataContract input)
        {
            await tagsCacheLock.WaitAsync();
            var tags = await ReadTags();
            tagsCacheLock.Release();

            await pointsCacheLock.WaitAsync();
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
            pointsCacheLock.Release();
            return Json(point.ToDataContract(tags));
        }

        [HttpGet("/api/tags")]
        public async Task<IActionResult> GetAllTags()
        {
            await tagsCacheLock.WaitAsync();
            var tags = await ReadTags();
            tagsCacheLock.Release();

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

            await tagsCacheLock.WaitAsync();
            var tags = await ReadTags();

            await pointsCacheLock.WaitAsync();
            var points = await ReadStorage(tags);
            pointsCacheLock.Release();

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
            tagsCacheLock.Release();
            return Json(tags.Values);
        }
        
        [HttpGet("/api/charts")]
        public async Task<IActionResult> GetAllCharts()
        {
            await chartsCacheLock.WaitAsync();
            
            string chartsString;
            var userId = (string)HttpContext.Items["UserId"];
            if (!chartsCache.TryGetValue(userId, out chartsString))
            {
                chartsString = await _storageClient.ReadChartsAsStringAsync();
                chartsCache[userId] = chartsString;
            }
            
            chartsCacheLock.Release();
            return Content(chartsString, "application/json");
        }

        [HttpPut("/api/charts")]
        public async Task<IActionResult> PutAllCharts()
        {
            var chartsString = await new StreamReader(this.Request.Body).ReadToEndAsync();
            await chartsCacheLock.WaitAsync();
            var userId = (string)HttpContext.Items["UserId"];
            chartsCache[userId] = chartsString;
            chartsCacheLock.Release();
            await _storageClient.WriteChartsAsStringAsync(chartsString);
            return Ok();
        }

        private async Task<Dictionary<Guid, Point>> ReadStorage(Dictionary<string, Tag> tags)
        {
            Dictionary<Guid, Point> points;
            string pointsString;
            var userId = (string)HttpContext.Items["UserId"];
            if (!pointsCache.TryGetValue(userId, out pointsString))
            {
                pointsString = await _storageClient.ReadPointsAsStringAsync();
                pointsCache[userId] = pointsString;
            }
            
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

            return points;
        }

        private async Task WriteStorage(
            Dictionary<Guid, Point> points,
            Dictionary<string, Tag> tags)
        {
            var pointsString = JsonConvert.SerializeObject(
                points.Values.Select(p => p.ToDataContract(tags)).ToList(), jsonSettings);

            var userId = (string)HttpContext.Items["UserId"];
            pointsCache[userId] = pointsString;
            await _storageClient.WritePointsAsStringAsync(pointsString);
        }

        private async Task<Dictionary<string, Tag>> ReadTags()
        {
            Dictionary<string, Tag> tags;
            string tagsString;
            var userId = (string)HttpContext.Items["UserId"];
            if (!tagsCache.TryGetValue(userId, out tagsString))
            {
                tagsString = await _storageClient.ReadTagsAsStringAsync();
                tagsCache[userId] = tagsString;
            }

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

            return tags;
        }

        private async Task WriteTags(Dictionary<string, Tag> tags)
        {
            var tagsDataContract = tags.Values.Select(tag => tag.ToDataContract());
            var tagsString = JsonConvert.SerializeObject(tagsDataContract, jsonSettings);
            var userId = (string)HttpContext.Items["UserId"];
            tagsCache[userId] = tagsString;
            await _storageClient.WriteTagsAsStringAsync(tagsString);
        }
    }
}
