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

namespace hutel.Controllers
{
    public class ApiController : Controller
    {
        private readonly ILogger _logger;
        private readonly Lazy<IStorageClient> _storageClientLazy;
        private IStorageClient _storageClient
        {
            get { return _storageClientLazy.Value; }
        }
        private const string _envUseGoogleStorage = "HUTEL_USE_GOOGLE_STORAGE";
        private const string _envUseGoogleDrive = "HUTEL_USE_GOOGLE_DRIVE";
        private const string _storagePath = "storage.json";
        private const string _storageBackupPath = "storage.json.bak";
        private const string _tagsPath = "tags.json";
        private const string _tagsBackupPath = "tags.json.bak";

        public ApiController(IMemoryCache memoryCache, ILogger<ApiController> logger)
        {
            if (Environment.GetEnvironmentVariable(_envUseGoogleDrive) == "1")
            {
                _storageClientLazy = new Lazy<IStorageClient>(() =>
                {
                    var userId = (string)HttpContext.Items["UserId"];
                    return new GoogleDriveStorageClient(userId);
                });
            }
            else if (Environment.GetEnvironmentVariable(_envUseGoogleStorage) == "1")
            {
                _storageClientLazy = new Lazy<IStorageClient>(() => new GoogleCloudStorageClient());
            }
            else
            {
                _storageClientLazy = new Lazy<IStorageClient>(() => new LocalStorageClient());
            }
            _logger = logger;
        }

        [HttpGet("/api/points")]
        public async Task<IActionResult> GetAllPoints(string startDate)
        {
            var tags = await ReadTags();
            var points = await ReadStorage(tags);
            var filteredPoints = points.Values
                    .Where(point => startDate == null || point.Date >= new HutelDate(startDate))
                    .ToList();
            filteredPoints.Sort((p1, p2) => p2.Date.DateTime.CompareTo(p1.Date.DateTime));
            return Json(filteredPoints.Select(p => p.ToDataContract(tags)));
        }

        [HttpPut("/api/points")]
        [ValidateModelState]
        public async Task<IActionResult> PutAllPoints([FromBody]PointsStorageDataContract replacementPoints)
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
                point = Point.FromDataContract(input, id, tags);
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
        public async Task<IActionResult> PutAllTags([FromBody]List<Tag> replacementTagsList)
        {
            if (!replacementTagsList.Any())
            {
                return new BadRequestObjectResult(
                    new InvalidOperationException("Tags list is empty"));
            }
            var tags = await ReadTags();
            var points = await ReadStorage(tags);
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

        private async Task<Dictionary<Guid, Point>> ReadStorage(Dictionary<string, Tag> tags)
        {
            if (await _storageClient.ExistsAsync(_storagePath))
            {
                var pointsString = await _storageClient.ReadAllAsync(_storagePath);
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
                return pointsDataContractList.ToDictionary(
                    point => point.Id,
                    point => Point.FromDataContract(point, tags));
            }
            else
            {
                return new Dictionary<Guid, Point>();
            }
        }

        private async Task WriteStorage(
            Dictionary<Guid, Point> points,
            Dictionary<string, Tag> tags)
        {
            if (await _storageClient.ExistsAsync(_storageBackupPath))
            {
                await _storageClient.DeleteAsync(_storageBackupPath);
            }
            if (await _storageClient.ExistsAsync(_storagePath))
            {
                await _storageClient.CopyAsync(_storagePath, _storageBackupPath);
            }
            var pointsJson = JsonConvert.SerializeObject(
                points.Values.Select(p => p.ToDataContract(tags)).ToList(),
                Formatting.Indented);
            await _storageClient.WriteAllAsync(_storagePath, pointsJson);
        }

        private async Task<Dictionary<string, Tag>> ReadTags()
        {
            if (!await _storageClient.ExistsAsync(_tagsPath))
            {
                throw new InvalidOperationException("Tags config doesn't exist");
            }
            var tagsString = await _storageClient.ReadAllAsync(_tagsPath);
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

        private async Task WriteTags(Dictionary<string, Tag> tags)
        {
            if (await _storageClient.ExistsAsync(_tagsBackupPath))
            {
                await _storageClient.DeleteAsync(_tagsBackupPath);
            }
            if (await _storageClient.ExistsAsync(_tagsPath))
            {
                await _storageClient.CopyAsync(_tagsPath, _tagsBackupPath);
            }
            var tagsDataContract = tags.Values.Select(tag => tag.ToDataContract());
            var tagsJson = JsonConvert.SerializeObject(tagsDataContract, Formatting.Indented);
            await _storageClient.WriteAllAsync(_tagsPath, tagsJson);
        }
    }
}
