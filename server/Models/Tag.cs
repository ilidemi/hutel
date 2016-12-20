using System;
using System.Collections.Generic;
using System.Reflection;

namespace hutel.Models
{
    public class Tag
    {
        public string Id { get; set; }

        public IDictionary<string, Field> Fields { get; set; }

        public class Field
        {
            public string Name { get; set; }

            public Type Type { get; set; }

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

        public void Validate(Point point)
        {
            if (point.TagId != this.Id)
            {
                throw new ValidationException($"Tag id mismatch: expected {this.Id}, got {point.TagId}");
            }
            
            foreach (var pointField in point.Extra.Keys)
            {
                if (!this.Fields.ContainsKey(pointField))
                {
                    throw new ValidationException($"Unknown property: {pointField}");
                }
            }
            foreach (var tagField in this.Fields.Values)
            {
                if (!point.Extra.ContainsKey(tagField.Name))
                {
                    throw new ValidationException($"Property not found: {tagField.Name}");
                }
                var pointFieldType = point.Extra[tagField.Name].GetType();
                if (pointFieldType != tagField.Type)
                {
                    throw new ValidationException(
                        $"Type mismatch for property {tagField.Name}: " +
                        $"expected {tagField.Type}, got {pointFieldType}");
                }
            }
        }
    }
}