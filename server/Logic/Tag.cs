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

        public static Tag FromDataContract(TagDataContract input)
        {
            if (string.IsNullOrEmpty(input.Id))
            {
                throw new TagValidationException("Tag id is empty");
            }
            foreach (var field in input.Fields)
            {
                if (Point.ReservedFields.Any(name => string.Compare(name, field.Name, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    throw new TagValidationException(
                        $"Tag {input.Id} contains reserved field {field.Name}");
                }
            }
            var duplicateFields = input.Fields
                .GroupBy(field => field.Name)
                .Where(g => g.Count() > 1);
            if (duplicateFields.Any())
            {
                var duplicatesString = string.Join(", ", duplicateFields);
                throw new TagValidationException(
                    $"Duplicate field names in tag {input.Id}: {duplicatesString}");
            }
            IEnumerable<BaseTagField> tagFields;
            try
            {
                tagFields = input.Fields.Select(field => BaseTagField.FromDataContract(field));
            }
            catch (Exception ex)
            {
                throw new TagValidationException(
                    $"Cannot initialize field in tag {input.Id}", ex);
            }

            return new Tag
            {
                Id = input.Id,
                Fields = tagFields.ToDictionary(
                    field => field.Name,
                    field => field)
            };
        }

        public TagDataContract ToDataContract()
        {
            return new TagDataContract
            {
                Id = Id,
                Fields = Fields.Values.Select(field => field.ToDataContract()).ToList()
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
