using System;
using System.Linq;
using System.Collections.Generic;
using hutel.Models;

namespace hutel.Logic
{
    public class Point
    {
        public static readonly IList<string> ReservedFields =
            new List<string> { "id", "tagId", "date", "submitTimestamp" };

        public Guid Id { get; set; }
        
        public string TagId { get; set; }
        
        public HutelDate Date { get; set; }

        public HutelTimestamp SubmitTimestamp { get; set; }

        public Dictionary<string, object> Extra { get; set; }

        public StoredPointDataContract ToDataContract(Dictionary<string, Tag> tags)
        {
            var tag = tags[TagId];
            var jsonExtra = Extra.ToDictionary(
                kvPair => kvPair.Key,
                kvPair => tag.Fields[kvPair.Key].ValueToDataContract(kvPair.Value));
            return new StoredPointDataContract
            {
                Id = Id,
                TagId = TagId,
                Date = Date.ToString(),
                SubmitTimestamp = SubmitTimestamp.ToString(),
                Extra = jsonExtra
            };
        }

        public static Point FromDataContract(
            PointDataContract input,
            Guid id,
            HutelTimestamp submitTimestamp,
            Dictionary<string, Tag> tags)
        {
            return FromFields(
                id, input.TagId, input.Date, submitTimestamp, input.Extra, tags);
        }

        public static Point FromDataContract(
            StoredPointDataContract input, Dictionary<string, Tag> tags)
        {
            try
            {
                var pointSubmitTimestamp = new HutelTimestamp(input.SubmitTimestamp);
                return FromFields(
                    input.Id, input.TagId, input.Date, pointSubmitTimestamp, input.Extra, tags);
            }
            catch (FormatException ex)
            {
                throw new PointValidationException(
                    $"Malformed timestamp: {input.SubmitTimestamp}", ex);
            }
        }

        private static Point FromFields(
            Guid id,
            string tagId,
            string date,
            HutelTimestamp submitTimestamp,
            Dictionary<string, Object> extra,
            Dictionary<string, Tag> tags)
        {
            if (!tags.ContainsKey(tagId))
            {
                throw new PointValidationException($"Unknown tag: {tagId}");
            }
            var tag = tags[tagId];
            if (extra != null)
            {
                foreach (var pointField in extra.Keys)
                {
                    if (!tag.Fields.ContainsKey(pointField))
                    {
                        throw new PointValidationException($"Unknown property: {pointField}");
                    }
                }
            }

            var pointExtra = new Dictionary<string, Object>();
            foreach (var tagField in tag.Fields.Values)
            {
                if (extra == null || !extra.ContainsKey(tagField.Name))
                {
                    throw new PointValidationException($"Property not found: {tagField.Name}");
                }
                try
                {
                    pointExtra.Add(
                        tagField.Name, tagField.ValueFromDataContract(extra[tagField.Name]));
                }
                catch(TypeValidationException ex)
                {
                    throw new PointValidationException(
                        $"Malformed property: {extra[tagField.Name]}", ex);
                }
            }

            try
            {
                var pointDate = new HutelDate(date);
                return new Point
                {
                    Id = id,
                    TagId = tagId,
                    Date = pointDate,
                    SubmitTimestamp = submitTimestamp,
                    Extra = pointExtra
                };
            }
            catch (FormatException ex)
            {
                throw new PointValidationException($"Malformed date: {date}", ex);
            }
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
