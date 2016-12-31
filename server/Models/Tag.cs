using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hutel.Logic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace hutel.Models
{
    [JsonConverter(typeof(TagJsonConverter))]
    public class Tag
    {
        public string Id { get; set; }

        public IDictionary<string, Field> Fields { get; set; }

        public class Field
        {
            public string Name { get; set; }

            public IFieldType Type { get; set; }

        }
    }

    public class TagJsonConverter : JsonConverter
    {
        private readonly Dictionary<string, Type> _stringToSimpleFieldType =
            new Dictionary<string, Type>
            {
                { "int", typeof(IntFieldType) },
                { "float", typeof(FloatFieldType) },
                { "string", typeof(StringFieldType) },
                { "date", typeof(DateFieldType) },
                { "time", typeof(TimeFieldType) }
            };

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override Object ReadJson(
            JsonReader reader, Type objectType, Object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var id = jObject
                .GetValue("id", StringComparison.OrdinalIgnoreCase)
                .Value<string>();
            var fieldsJson = (JArray)jObject.GetValue("fields", StringComparison.OrdinalIgnoreCase);
            var fieldsList = fieldsJson.Select(fieldJson => DeserializeField((JObject)fieldJson));
            foreach (var field in fieldsList)
            {
                if (Point.ReservedFields.Any(name => string.Compare(name, field.Name, true) == 0))
                {
                    throw new JsonReaderException($"Tag {id} contains reserved field {field.Name}");
                }
            }
            var duplicateFields = fieldsList
                .Select(field => field.Name)
                .GroupBy(name => name)
                .Where(g => g.Count() > 1);
            if (duplicateFields.Any())
            {
                throw new InvalidOperationException(
                    $"Duplicate field names in tag {id}: {string.Join(", ", duplicateFields)}");
            }
            var fields = fieldsList.ToDictionary(field => field.Name, field => field);
            return new Tag
            {
                Id = id,
                Fields = fields
            };
        }

        public override void WriteJson(JsonWriter writer, Object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Tag.Field).IsAssignableFrom(objectType);
        }

        private Tag.Field DeserializeField(JObject jObject)
        {
            var name = jObject
                .GetValue("name", StringComparison.OrdinalIgnoreCase)
                .Value<string>();
            var rawType = jObject.GetValue("type", StringComparison.OrdinalIgnoreCase);
            if (rawType.Type == JTokenType.String)
            {
                var rawString = rawType.Value<string>();
                Type type;
                if (!_stringToSimpleFieldType.TryGetValue(rawString, out type))
                {
                    throw new JsonReaderException($"Unknown type {rawString} in tag field {name}");
                }
                var typeInstance = (IFieldType)Activator.CreateInstance(type);
                return new Tag.Field
                {
                    Name = name,
                    Type = typeInstance
                };
            }
            else if (rawType.Type == JTokenType.Array)
            {
                var values = rawType.ToObject<List<string>>();
                if (!values.Any())
                {
                    throw new JsonReaderException($"Enum field {name} is empty");
                }
                return new Tag.Field
                {
                    Name = name,
                    Type = new EnumFieldType(values)
                };
            }
            else
            {
                throw new JsonReaderException($"Invalid token type in tag field {name}");
            }
        }
    }
}
