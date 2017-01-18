using System;
using System.Collections.Generic;
using System.Linq;
using hutel.Logic;
using hutel.Models;
using Newtonsoft.Json;
using Xunit;
using static hutel.Tests.Constants;

namespace hutel.Tests
{
    public class TagDeserializationTests
    {
        [Fact]
        public void ValidCompleteTag()
        {
            var json = $@"{{
                'id': 'completeTag',
                'fields': [
                    {{
                        'name': '{IntFieldName}',
                        'type': 'int'
                    }},
                    {{
                        'name': '{FloatFieldName}',
                        'type': 'float'
                    }},
                    {{
                        'name': '{StringFieldName}',
                        'type': 'string'
                    }},
                    {{
                        'name': '{DateFieldName}',
                        'type': 'date'
                    }},
                    {{
                        'name': '{TimeFieldName}',
                        'type': 'time'
                    }},
                    {{
                        'name': '{EnumFieldName}',
                        'type': [
                            '{EnumValueA}',
                            '{EnumValueB}'
                        ]
                    }}
                ]
            }}";
            var tag = JsonConvert.DeserializeObject<Tag>(json);
            Assert.Equal(tag.Id, "completeTag");
            Assert.Contains(IntFieldName, tag.Fields.Keys);
            Assert.IsType<IntFieldType>(tag.Fields[IntFieldName].Type);
            Assert.Contains(FloatFieldName, tag.Fields.Keys);
            Assert.IsType<FloatFieldType>(tag.Fields[FloatFieldName].Type);
            Assert.Contains(StringFieldName, tag.Fields.Keys);
            Assert.IsType<StringFieldType>(tag.Fields[StringFieldName].Type);
            Assert.Contains(DateFieldName, tag.Fields.Keys);
            Assert.IsType<DateFieldType>(tag.Fields[DateFieldName].Type);
            Assert.Contains(TimeFieldName, tag.Fields.Keys);
            Assert.IsType<TimeFieldType>(tag.Fields[TimeFieldName].Type);
            Assert.Contains(EnumFieldName, tag.Fields.Keys);
            Assert.IsType<EnumFieldType>(tag.Fields[EnumFieldName].Type);
            var enumFieldType = tag.Fields[EnumFieldName].Type as EnumFieldType;
            Assert.Contains(EnumValueA, enumFieldType.Values);
            Assert.Contains(EnumValueB, enumFieldType.Values);
        }

        [Theory]
        [InlineData("{ 'fields': [{ 'name': 'f', 'type': 'int' }] }")]
        [InlineData("{ 'id': 'id' }")]
        [InlineData("{ 'id': 'id', 'fields': [] }")]
        [InlineData("{ 'id': 'id', 'fields': [{ 'name': 'f', 'type': 'int' }, { 'name': 'f', 'type': 'float' }] }")]
        [InlineData("{ 'id': 'id', 'fields': [{ 'name': 'f', 'type': 'unknownType' }] }")]
        [InlineData("{ 'id': 'id', 'fields': [{ 'name': 'f', 'type': [] }] }")]
        [InlineData("{ 'id': 'id', 'fields': [{ 'name': 'f', 'type': {} }] }")]
        [InlineData("{ 'unknownField: '', 'id': 'id', 'fields': [{ 'name': 'f', 'type': 'int' }] }")]
        [InlineData("{ 'id': 'id', 'fields': [{ 'unknownField: '', 'name': 'f', 'type': 'int' }] }")]
        [InlineData("{ 'id': 'id', 'fields': [{ 'type': 'int' }] }")]
        [InlineData("{ 'id': 'id', 'fields': [{ 'name': 'f' }] }")]
        public void InvalidTag(string json)
        {
            Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<Tag>(json));
        }

        public static IEnumerable<object[]> TagWithReservedFieldData
        {
            get
            {
                return Point.ReservedFields.Select(field => new[]{ field });
            }
        }

        [Theory]
        [MemberDataAttribute(nameof(TagWithReservedFieldData))]
        public void TagWithReservedField(string fieldName)
        {
            var json = $"{{ 'id': 'id', 'fields': [{{ 'name': {fieldName}, 'type': 'int' }}] }}";
            Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<Tag>(json));
        }
    }
}