using System;
using System.Collections.Generic;
using hutel.Filters;
using hutel.Logic;
using hutel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace hutel.Controllers
{
    [Route("api/[controller]")]
    public class PointsController : Controller
    {
        private IMemoryCache _memoryCache;
        private const string _pointsKey = "points";
        private const string _tagsKey = "tags";

        public PointsController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            Dictionary<Guid, Point> points;
            if (!_memoryCache.TryGetValue(_pointsKey, out points))
            {
                _memoryCache.Set(
                    _pointsKey,
                    new Dictionary<Guid, Point>(),
                    new MemoryCacheEntryOptions()
                        .SetPriority(CacheItemPriority.NeverRemove)
                );
            }
            Dictionary<string, Tag> tags;
            if (!memoryCache.TryGetValue(_tagsKey, out tags))
            {
                _memoryCache.Set(
                    _tagsKey,
                    CreateTags(),
                    new MemoryCacheEntryOptions()
                        .SetPriority(CacheItemPriority.NeverRemove)
                );
            }
        }

        // POST api/points/id
        [HttpPost("{id}")]
        [ValidateModelState]
        public IActionResult Post(Guid id, [FromBody]Point point)
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
            return Json(points);
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
    }
}