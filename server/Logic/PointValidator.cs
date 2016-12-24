using System;
using hutel.Models;

namespace hutel.Logic
{
    public class PointValidator
    {
        public static void Validate(Point point, Tag tag)
        {
            if (tag == null)
            {
                throw new ValidationException($"Unknown tag: {point.TagId}");
            }
            if (point.TagId != tag.Id)
            {
                throw new ValidationException($"Tag id mismatch: expected {tag.Id}, got {point.TagId}");
            }
            
            foreach (var pointField in point.Extra.Keys)
            {
                if (!tag.Fields.ContainsKey(pointField))
                {
                    throw new ValidationException($"Unknown property: {pointField}");
                }
            }
            foreach (var tagField in tag.Fields.Values)
            {
                if (!point.Extra.ContainsKey(tagField.Name))
                {
                    throw new ValidationException($"Property not found: {tagField.Name}");
                }
                if (!tagField.Type.TypeIsValid(point.Extra[tagField.Name]))
                {
                    throw new ValidationException($"Type mismatch for property {tagField.Name}");
                }
            }
        }
    }
    public class ValidationException: Exception
    {
        public ValidationException()
        {
        }

        public ValidationException(string message): base(message)
        {
        }

        public ValidationException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}