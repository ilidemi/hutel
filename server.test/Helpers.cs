using System.Collections.Generic;
using hutel.Logic;
using hutel.Models;
using static hutel.Tests.Constants;

namespace hutel.Tests
{
    public static class Helpers
    {
        public static IntTagField CreateIntField()
        {
            var tagFieldJson = new TagFieldJson
            {
                Name = IntFieldName,
                Type = TagFieldConstants.Int
            };
            return new IntTagField(tagFieldJson);
        }

        public static FloatTagField CreateFloatField()
        {
            var tagFieldJson = new TagFieldJson
            {
                Name = FloatFieldName,
                Type = TagFieldConstants.Float
            };
            return new FloatTagField(tagFieldJson);
        }

        public static StringTagField CreateStringField()
        {
            var tagFieldJson = new TagFieldJson
            {
                Name = StringFieldName,
                Type = TagFieldConstants.String
            };
            return new StringTagField(tagFieldJson);
        }

        public static DateTagField CreateDateField()
        {
            var tagFieldJson = new TagFieldJson
            {
                Name = DateFieldName,
                Type = TagFieldConstants.Date
            };
            return new DateTagField(tagFieldJson);
        }

        public static TimeTagField CreateTimeField()
        {
            var tagFieldJson = new TagFieldJson
            {
                Name = TimeFieldName,
                Type = TagFieldConstants.Time
            };
            return new TimeTagField(tagFieldJson);
        }

        public static EnumTagField CreateEnumField()
        {
            var tagFieldJson = new TagFieldJson
            {
                Name = EnumFieldName,
                Type = TagFieldConstants.Enum,
                Values = new List<string>{ EnumValueA, EnumValueB }
            };
            return new EnumTagField(tagFieldJson);
        }
    }
}