using System;
using System.Linq;
using System.Collections.Generic;
using hutel.Models;

namespace hutel.Logic
{
    public class Point
    {
        public static readonly IList<string> ReservedFields =
            new List<string> { "id", "tagId", "date" };

        public Guid Id { get; set; }
        
        public string TagId { get; set; }
        
        public HutelDate Date { get; set; }

        public Dictionary<string, object> Extra { get; set; }

        public PointWithIdJson ToJson(Dictionary<string, Tag> tags)
        {
            var tag = tags[TagId];
            var jsonExtra = Extra.ToDictionary(
                kvPair => kvPair.Key,
                kvPair => tag.Fields[kvPair.Key].Type.ToJson(kvPair.Value));
            return new PointWithIdJson
            {
                Id = Id,
                TagId = TagId,
                Date = Date.ToString(),
                Extra = jsonExtra
            };
        }

        public static Point FromJson(PointJson pointJson, Guid id, Dictionary<string, Tag> tags)
        {
            return FromFields(id, pointJson.TagId, pointJson.Date, pointJson.Extra, tags);
        }

        public static Point FromJson(PointWithIdJson pointJson, Dictionary<string, Tag> tags)
        {
            return FromFields(pointJson.Id, pointJson.TagId, pointJson.Date, pointJson.Extra, tags);
        }

        private static Point FromFields(
            Guid id,
            string tagId,
            string date,
            Dictionary<string, Object> extra,
            Dictionary<string, Tag> tags)
        {
            if (!tags.ContainsKey(tagId))
            {
                throw new PointValidationException($"Unknown tag: {tagId}");
            }
            var tag = tags[tagId];
            foreach (var pointField in extra.Keys)
            {
                if (!tag.Fields.ContainsKey(pointField))
                {
                    throw new PointValidationException($"Unknown property: {pointField}");
                }
            }
            var pointExtra = new Dictionary<string, Object>();
            foreach (var tagField in tag.Fields.Values)
            {
                if (!extra.ContainsKey(tagField.Name))
                {
                    throw new PointValidationException($"Property not found: {tagField.Name}");
                }
                pointExtra.Add(tagField.Name, tagField.Type.FromJson(extra[tagField.Name]));
            }
            var pointDate = new HutelDate(date);
            return new Point
            {
                Id = id,
                TagId = tagId,
                Date = pointDate,
                Extra = pointExtra
            };
        }
    }
    public class PointValidationException: Exception
    {
        public PointValidationException()
        {
        }

        public PointValidationException(string message): base(message)
        {
        }

        public PointValidationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
