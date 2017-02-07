using System;
using System.Collections.Generic;
using hutel.Logic;
using hutel.Models;
using Xunit;
using static hutel.Tests.Constants;
using static hutel.Tests.Helpers;

namespace hutel.Tests
{
    public class TagFieldTests
    {
        [Fact]
        public void EmptyEnum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnumTagField(new TagFieldJson
                {
                    Name = EnumFieldName,
                    Type = TagFieldConstants.Enum,
                    Values =  new List<string>()
                }));
        }

        [Fact]
        public void ValidIntJson()
        {
            var intField = CreateIntField();
            var input = (Int64)0;
            Assert.Equal(intField.ValueFromJson(input), input);
            Assert.Equal(intField.ValueToJson(input), input);
        }

        [Fact]
        public void ValidFloatJson()
        {
            var floatField = CreateFloatField();
            var input = (Double)0.0;
            Assert.Equal(floatField.ValueFromJson(input), input);
            Assert.Equal(floatField.ValueToJson(input), input);
        }

        [Fact]
        public void ValidStringJson()
        {
            var stringField = CreateStringField();
            var input = "";
            Assert.Equal(stringField.ValueFromJson(input), input);
            Assert.Equal(stringField.ValueToJson(input), input);
        }

        [Fact]
        public void ValidDateJson()
        {
            var dateField = CreateDateField();
            var input = "2000-01-01";
            var date = dateField.ValueFromJson(input);
            var hutelDate = date as HutelDate;
            Assert.NotNull(hutelDate);
            Assert.Equal(hutelDate.DateTime, new DateTime(2000, 1, 1));
            Assert.Equal(dateField.ValueToJson(hutelDate), input);
        }

        public static IEnumerable<Object[]> ValidTimeJsonData
        {
            get
            {
                return new[]
                {
                    new Object[] { "10:11:12", new TimeSpan(10, 11, 12) },
                    new Object[] { "09:01:02", new TimeSpan(9, 1, 2) }
                };
            }
        }

        [Theory]
        [MemberData(nameof(ValidTimeJsonData))]
        public void ValidTimeJson(string input, TimeSpan expectedValue)
        {
            var timeField = CreateTimeField();
            var time = timeField.ValueFromJson(input);
            var hutelTime = time as HutelTime;
            Assert.NotNull(hutelTime);
            Assert.Equal(hutelTime.TimeSpan, expectedValue);
            Assert.Equal(timeField.ValueToJson(hutelTime), input);
        }

        [Fact]
        public void EnumFromJson()
        {
            var enumField = CreateEnumField();
            Assert.Equal(enumField.ValueFromJson(EnumValueA), EnumValueA);
            Assert.Equal(enumField.ValueFromJson(EnumValueB), EnumValueB);
            Assert.Throws<TypeValidationException>(() => enumField.ValueFromJson(0));
            Assert.Throws<TypeValidationException>(() => enumField.ValueFromJson(""));
            Assert.Throws<TypeValidationException>(() => enumField.ValueFromJson("c"));
        }

        [Fact]
        public void EnumToJson()
        {
            var enumField = CreateEnumField();
            Assert.Equal(enumField.ValueToJson(EnumValueA), EnumValueA);
            Assert.Equal(enumField.ValueToJson(EnumValueB), EnumValueB);
            Assert.Throws<TypeValidationException>(() => enumField.ValueToJson(0));
            Assert.Throws<TypeValidationException>(() => enumField.ValueToJson(""));
            Assert.Throws<TypeValidationException>(() => enumField.ValueToJson("c"));
        }

        public static IEnumerable<Object[]> InvalidFieldData
        {
            get
            {
                return new[]
                {
                    new Object[] { CreateIntField(), "" },
                    new Object[] { CreateFloatField(), "" },
                    new Object[] { CreateStringField(), 0 },
                    new Object[] { CreateDateField(), "01/02/2000" },
                    new Object[] { CreateDateField(), "01012000" },
                    new Object[] { CreateDateField(), "2017-09-31" },
                    new Object[] { CreateDateField(), "" },
                    new Object[] { CreateDateField(), 0 },
                    new Object[] { CreateTimeField(), "9:1:2" },
                    new Object[] { CreateTimeField(), "1.10:11:12" },
                    new Object[] { CreateTimeField(), "30:11:12" },
                    new Object[] { CreateTimeField(), "24:00:00" },
                    new Object[] { CreateTimeField(), "00:60:00" }
                };
            }
        }
        
        [Theory]
        [MemberData(nameof(InvalidFieldData))]
        public void InvalidFieldFromJson(BaseTagField field, Object input)
        {
            Assert.Throws<TypeValidationException>(() => field.ValueFromJson(input));
        }
    }
}
