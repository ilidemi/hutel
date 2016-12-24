using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        public interface IFieldType
        {
            bool TypeIsValid(object obj);
        }

        public class FInt : IFieldType
        {
            public bool TypeIsValid(object obj)
            {
                return obj is Int64;
            }
        }

        public class FFloat : IFieldType
        {
            public bool TypeIsValid(object obj)
            {
                return obj is Double;
            }
        }

        public class FString : IFieldType
        {
            public bool TypeIsValid(object obj)
            {
                return obj is string;
            }
        }

        public class FDate : IFieldType
        {
            public bool TypeIsValid(object obj)
            {
                var date = obj as DateTime?;
                return date.HasValue && date.Value.TimeOfDay.TotalMilliseconds == 0;
            }
        }
        
        public class FTime : IFieldType
        {
            public bool TypeIsValid(object obj)
            {
                var date = obj as DateTime?;
                return date.HasValue && date.Value.Date == date.Value;
            }
        }

        public class FEnum : IFieldType
        {
            private readonly IList<string> _values;

            public FEnum(IList<string> values)
            {
                _values = values;
            }

            public bool TypeIsValid(object obj)
            {
                var str = obj as string;
                return str != null && _values.Contains(str);
            }
        }
    }

    public class TagJsonConverter : JsonConverter
    {
        private readonly Dictionary<string, Type> _stringToSimpleFieldType =
            new Dictionary<string, Type>
            {
                { "int", typeof(Tag.FInt) },
                { "float", typeof(Tag.FFloat) },
                { "string", typeof(Tag.FString) },
                { "date", typeof(Tag.FDate) },
                { "time", typeof(Tag.FTime) }
            };

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override object ReadJson(
            JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jObject = JObject.Load(reader);
            var id = jObject
                .GetValue("id", StringComparison.OrdinalIgnoreCase)
                .Value<string>();
            var fieldsJson = (JArray)jObject.GetValue("fields", StringComparison.OrdinalIgnoreCase);
            var fieldsList = fieldsJson.Select(fieldJson => DeserializeField((JObject)fieldJson));
            foreach (var field in fieldsList)
            {
                if (Point.ReservedFields.Any(name => name == field.Name))
                {
                    throw new JsonReaderException($"Tag {id} contains reserved field {field.Name}");
                }
            }
            var fields = fieldsList.ToDictionary(field => field.Name, field => field);
            return new Tag
            {
                Id = id,
                Fields = fields
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
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
                var typeInstance = (Tag.IFieldType)Activator.CreateInstance(type);
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
                    Type = new Tag.FEnum(values)
                };
            }
            else
            {
                throw new JsonReaderException($"Invalid token type in tag field {name}");
            }
        }
    }
}