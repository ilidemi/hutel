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
                        'type': '{TagFieldConstants.IntType}'
                    }},
                    {{
                        'name': '{FloatFieldName}',
                        'type': '{TagFieldConstants.FloatType}'
                    }},
                    {{
                        'name': '{StringFieldName}',
                        'type': '{TagFieldConstants.StringType}'
                    }},
                    {{
                        'name': '{DateFieldName}',
                        'type': '{TagFieldConstants.DateType}'
                    }},
                    {{
                        'name': '{TimeFieldName}',
                        'type': '{TagFieldConstants.TimeType}'
                    }},
                    {{
                        'name': '{EnumFieldName}',
                        'type': '{TagFieldConstants.EnumType}',
                        'values': [
                            '{EnumValueA}',
                            '{EnumValueB}'
                        ]
                    }}
                ]
            }}";
            var tagDataContract = JsonConvert.DeserializeObject<TagDataContract>(json);
            var tag = Tag.FromDataContract(tagDataContract);
            Assert.Equal("completeTag", tag.Id);
            Assert.Contains(IntFieldName, tag.Fields.Keys);
            Assert.IsType<IntTagField>(tag.Fields[IntFieldName]);
            Assert.Contains(FloatFieldName, tag.Fields.Keys);
            Assert.IsType<FloatTagField>(tag.Fields[FloatFieldName]);
            Assert.Contains(StringFieldName, tag.Fields.Keys);
            Assert.IsType<StringTagField>(tag.Fields[StringFieldName]);
            Assert.Contains(DateFieldName, tag.Fields.Keys);
            Assert.IsType<DateTagField>(tag.Fields[DateFieldName]);
            Assert.Contains(TimeFieldName, tag.Fields.Keys);
            Assert.IsType<TimeTagField>(tag.Fields[TimeFieldName]);
            Assert.Contains(EnumFieldName, tag.Fields.Keys);
            Assert.IsType<EnumTagField>(tag.Fields[EnumFieldName]);
            var enumFieldType = tag.Fields[EnumFieldName] as EnumTagField;
            Assert.Contains(EnumValueA, enumFieldType.Values);
            Assert.Contains(EnumValueB, enumFieldType.Values);
        }

        [Fact]
        public void ValidEmptyTag()
        {
            var json = "{ 'id': 'id', 'fields': [] }";
            var tagDataContract = JsonConvert.DeserializeObject<TagDataContract>(json);
            var tag = Tag.FromDataContract(tagDataContract);
            Assert.Equal("id", tag.Id);
            Assert.Empty(tag.Fields);
        }

        [Theory]
        [InlineData("{ 'id': 'id', 'fields': [{ 'name': 'f', 'type': 'int' }, { 'name': 'f', 'type': 'float' }] }")]
        [InlineData("{ 'id': 'id', 'fields': [{ 'name': 'f', 'type': 'unknownType' }] }")]
        public void InvalidTag(string json)
        {
            Assert.Throws<TagValidationException>(() =>
                {
                    var tagDataContract = JsonConvert.DeserializeObject<TagDataContract>(json);
                    Tag.FromDataContract(tagDataContract);
                });
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
            var json = $@"{{
                'id': 'id',
                'fields': [
                    {{
                        'name': '{fieldName}',
                        'type': '{TagFieldConstants.IntType}'
                    }}
                ]
            }}";
            Assert.Throws<TagValidationException>(() => 
                {
                    var tagDataContract = JsonConvert.DeserializeObject<TagDataContract>(json);
                    Tag.FromDataContract(tagDataContract);
                });
        }
    }
}