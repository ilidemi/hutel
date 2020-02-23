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
            var fieldDataContract = new TagFieldDataContract
            {
                Name = IntFieldName,
                Type = TagFieldConstants.IntType
            };
            return new IntTagField(fieldDataContract);
        }

        public static FloatTagField CreateFloatField()
        {
            var fieldDataContract = new TagFieldDataContract
            {
                Name = FloatFieldName,
                Type = TagFieldConstants.FloatType
            };
            return new FloatTagField(fieldDataContract);
        }

        public static StringTagField CreateStringField()
        {
            var fieldDataContract = new TagFieldDataContract
            {
                Name = StringFieldName,
                Type = TagFieldConstants.StringType
            };
            return new StringTagField(fieldDataContract);
        }

        public static DateTagField CreateDateField()
        {
            var fieldDataContract = new TagFieldDataContract
            {
                Name = DateFieldName,
                Type = TagFieldConstants.DateType
            };
            return new DateTagField(fieldDataContract);
        }

        public static TimeTagField CreateTimeField()
        {
            var fieldDataContract = new TagFieldDataContract
            {
                Name = TimeFieldName,
                Type = TagFieldConstants.TimeType
            };
            return new TimeTagField(fieldDataContract);
        }

        public static EnumTagField CreateEnumField()
        {
            var fieldDataContract = new TagFieldDataContract
            {
                Name = EnumFieldName,
                Type = TagFieldConstants.EnumType,
                Values = new List<string>{ EnumValueA, EnumValueB }
            };
            return new EnumTagField(fieldDataContract);
        }
    }
}