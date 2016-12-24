using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using hutel.Filters;
using hutel.Logic;
using hutel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace hutel.Controllers
{
    [Route("api/[controller]")]
    public class PointsController : Controller
    {
        private IMemoryCache _memoryCache;
        private const string _pointsKey = "points";
        private const string _tagsKey = "tags";
        private const string _storagePath = ".\\server\\storage.json";
        private const string _storageBackupPath = ".\\server\\storage.json.bak";

        public PointsController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            Dictionary<Guid, Point> points;
            if (!_memoryCache.TryGetValue(_pointsKey, out points))
            {
                points = ReadStorage();
                _memoryCache.Set(
                    _pointsKey,
                    points,
                    new MemoryCacheEntryOptions()
                        .SetPriority(CacheItemPriority.NeverRemove)
                );
            }
            Dictionary<string, Tag> tags;
            if (!memoryCache.TryGetValue(_tagsKey, out tags))
            {
                tags = CreateTags();
                _memoryCache.Set(
                    _tagsKey,
                    tags,
                    new MemoryCacheEntryOptions()
                        .SetPriority(CacheItemPriority.NeverRemove)
                );
            }
        }

        // GET /api/points
        [HttpGet]
        public IActionResult GetAll()
        {
            var points = _memoryCache.Get<Dictionary<Guid, Point>>(_pointsKey);
            return Json(points.Values);
        }

        // POST /api/points
        [HttpPost]
        [ValidateModelState]
        public IActionResult PostAll([FromBody]List<Point> replacementPoints)
        {
            var points = _memoryCache.Get<Dictionary<Guid, Point>>(_pointsKey);
            var tags = _memoryCache.Get<Dictionary<string, Tag>>(_tagsKey);
            try
            {
                foreach (var point in replacementPoints)
                {
                    if (!tags.ContainsKey(point.TagId))
                    {
                        throw new ValidationException($"Unknown tag: {point.TagId}");
                    }
                    PointValidator.Validate(point, tags[point.TagId]);
                }
            }
            catch(ValidationException ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
            points.Clear();
            foreach (var p in replacementPoints)
            {
                points.Add(p.Id, p);
            }
            WriteStorage(points);
            return Json(points.Values);
        }

        // POST api/points/id
        [HttpPost("{id}")]
        [ValidateModelState]
        public IActionResult PostOne(Guid id, [FromBody]Point point)
        {
            var points = _memoryCache.Get<Dictionary<Guid, Point>>(_pointsKey);
            var tags = _memoryCache.Get<Dictionary<string, Tag>>(_tagsKey);
            try
            {
                if (!tags.ContainsKey(point.TagId))
                {
                    throw new ValidationException($"Unknown tag: {point.TagId}");
                }
                PointValidator.Validate(point, tags[point.TagId]);
            }
            catch(ValidationException ex)
            {
                return new BadRequestObjectResult(ex.ToString());
            }
            points[id] = point;
            WriteStorage(points);
            return Json(points.Values);
        }

        private static Dictionary<string, Tag> CreateTags()
        {
            return new Dictionary<string, Tag>
            {
                {
                    "testtag",
                    new Tag
                    {
                        Id = "testtag",
                        Fields = new Dictionary<string, Tag.Field>
                        {
                            {
                                "int",
                                new Tag.Field
                                {
                                    Name = "int",
                                    Type = Type.GetType("System.Int64")
                                }
                            },
                            {
                                "dateTime",
                                new Tag.Field
                                {
                                    Name = "dateTime",
                                    Type = Type.GetType("System.DateTime")
                                }
                            },
                            {
                                "string",
                                new Tag.Field
                                {
                                    Name = "string",
                                    Type = Type.GetType("System.String")
                                }
                            },
                            {
                                "float",
                                new Tag.Field
                                {
                                    Name = "float",
                                    Type = Type.GetType("System.Double")
                                }
                            }
                        }
                    }
                }
            };
        }

        private static Dictionary<Guid, Point> ReadStorage()
        {
            if (System.IO.File.Exists(_storagePath))
            {
                var pointsString = System.IO.File.ReadAllText(_storagePath);
                return JsonConvert
                    .DeserializeObject<List<Point>>(pointsString)
                    .ToDictionary(point => point.Id, point => point);
            }
            else
            {
                return new Dictionary<Guid, Point>();
            }
        }

        private static void WriteStorage(Dictionary<Guid, Point> points)
        {
            if (System.IO.File.Exists(_storageBackupPath))
            {
                System.IO.File.Delete(_storageBackupPath);
            }
            if (System.IO.File.Exists(_storagePath))
            {
                System.IO.File.Copy(_storagePath, _storageBackupPath);
            }
            var pointsJson = JsonConvert.SerializeObject(points.Values, Formatting.Indented);
            System.IO.File.WriteAllText(_storagePath, pointsJson);
        }
    }
}