using System;
using System.Collections.Generic;
using System.Linq;
using hutel.Models;

namespace hutel.Logic
{
    public class Tag
    {
        public string Id { get; set; }

        public IDictionary<string, BaseTagField> Fields { get; set; }

        public static Tag FromJson(TagJson tagJson)
        {
            if (tagJson.Id == string.Empty)
            {
                throw new TagValidationException("Tag id is empty");
            }
            if (!tagJson.Fields.Any())
            {
                throw new TagValidationException("Tag doesn't contain any fields");
            }
            foreach (var field in tagJson.Fields)
            {
                if (Point.ReservedFields.Any(name => string.Compare(name, field.Name, true) == 0))
                {
                    throw new TagValidationException(
                        $"Tag {tagJson.Id} contains reserved field {field.Name}");
                }
            }
            var duplicateFields = tagJson.Fields
                .GroupBy(field => field.Name)
                .Where(g => g.Count() > 1);
            if (duplicateFields.Any())
            {
                var duplicatesString = string.Join(", ", duplicateFields);
                throw new TagValidationException(
                    $"Duplicate field names in tag {tagJson.Id}: {duplicatesString}");
            }
            IEnumerable<BaseTagField> tagFields;
            try
            {
                tagFields = tagJson.Fields.Select(field => BaseTagField.FromJson(field));
            }
            catch (Exception ex)
            {
                throw new TagValidationException(
                    $"Cannot initialize field in tag {tagJson.Id}", ex);
            }

            return new Tag
            {
                Id = tagJson.Id,
                Fields = tagFields.ToDictionary(
                    field => field.Name,
                    field => field)
            };
        }

        public TagJson ToJson()
        {
            return new TagJson
            {
                Id = Id,
                Fields = Fields.Values.Select(field => field.ToJson()).ToList()
            };
        }
    }

    public class TagValidationException: Exception
    {
        public TagValidationException()
        {
        }

        public TagValidationException(string message): base(message)
        {
        }

        public TagValidationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
