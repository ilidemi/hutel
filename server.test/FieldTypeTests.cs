using System;
using System.Collections.Generic;
using hutel.Logic;
using Xunit;

namespace hutel.Tests
{
    public class FieldTypeTests
    {
        [Fact]
        public void EmptyEnum()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnumFieldType(new string[] {}));
        }

        [Theory]
        [InlineData(typeof(IntFieldType), (Int64)0)]
        [InlineData(typeof(FloatFieldType), (Double)0.0)]
        [InlineData(typeof(StringFieldType), "")]
        public void ValidTrivialTypeFromJson(Type fieldTypeType, Object input)
        {
            var fieldType = (IFieldType)Activator.CreateInstance(fieldTypeType);
            Assert.Equal(fieldType.FromJson(input), input);
        }

        [Fact]
        public void ValidDateFromJson()
        {
            var dateFieldType = new DateFieldType();
            var date = dateFieldType.FromJson("2000-01-01");
            var hutelDate = date as HutelDate;
            Assert.NotNull(hutelDate);
            Assert.Equal(hutelDate.DateTime, new DateTime(2000, 1, 1));
        }

        public static IEnumerable<Object[]> ValidTimeFromJsonData
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
        [MemberData(nameof(ValidTimeFromJsonData))]
        public void ValidTimeFromJson(string input, TimeSpan expectedValue)
        {
            var timeFieldType = new TimeFieldType();
            var time = timeFieldType.FromJson(input);
            var hutelTime = time as HutelTime;
            Assert.NotNull(hutelTime);
            Assert.Equal(hutelTime.TimeSpan, expectedValue);
        }

        [Fact]
        public void EnumFromJson()
        {
            var enumType = new EnumFieldType(new[] { "a", "b" });
            Assert.Equal(enumType.FromJson("a"), "a");
            Assert.Equal(enumType.FromJson("b"), "b");
            Assert.Throws<TypeValidationException>(() => enumType.FromJson(0));
            Assert.Throws<TypeValidationException>(() => enumType.FromJson(""));
            Assert.Throws<TypeValidationException>(() => enumType.FromJson("c"));
        }

        public static IEnumerable<Object[]> InvalidTypeData
        {
            get
            {
                return new[]
                {
                    new Object[] { typeof(IntFieldType), "" },
                    new Object[] { typeof(FloatFieldType), "" },
                    new Object[] { typeof(StringFieldType), 0 },
                    new Object[] { typeof(DateFieldType), "01/02/2000" },
                    new Object[] { typeof(DateFieldType), "01012000" },
                    new Object[] { typeof(DateFieldType), "" },
                    new Object[] { typeof(DateFieldType), 0 },
                    new Object[] { typeof(TimeFieldType), "9:1:2" },
                    new Object[] { typeof(TimeFieldType), "1.10:11:12" },
                    new Object[] { typeof(TimeFieldType), "30:11:12" }
                };
            }
        }
        
        [Theory]
        [MemberData(nameof(InvalidTypeData))]
        public void InvalidTypeFromJson(Type fieldTypeType, Object input)
        {
            var fieldType = (IFieldType)Activator.CreateInstance(fieldTypeType);
            Assert.Throws<TypeValidationException>(() => fieldType.FromJson(input));
        }

        [Theory]
        [InlineData(typeof(IntFieldType), (Int64)0)]
        [InlineData(typeof(FloatFieldType), (Double)0.0)]
        [InlineData(typeof(StringFieldType), "")]
        public void ValidTrivialTypeToJson(Type fieldTypeType, Object input)
        {
            var fieldType = (IFieldType)Activator.CreateInstance(fieldTypeType);
            Assert.Equal(fieldType.ToJson(input), input);
        }

        [Fact]
        public void ValidDateToJson()
        {
            var dateType = new DateFieldType();
            Assert.Equal(dateType.ToJson(new HutelDate("2000-10-01")), "2000-10-01");
        }

        [Fact]
        public void ValidTimeToJson()
        {
            var timeType = new TimeFieldType();
            Assert.Equal(timeType.ToJson(new HutelTime("10:09:08")), "10:09:08");
        }

        [Fact]
        public void EnumToJson()
        {
            var enumType = new EnumFieldType(new[] { "a", "b" });
            Assert.Equal(enumType.ToJson("a"), "a");
            Assert.Equal(enumType.ToJson("b"), "b");
            Assert.Throws<TypeValidationException>(() => enumType.ToJson(0));
            Assert.Throws<TypeValidationException>(() => enumType.ToJson(""));
            Assert.Throws<TypeValidationException>(() => enumType.ToJson("c"));
        }
        
        [Theory]
        [MemberData(nameof(InvalidTypeData))]
        public void InvalidTypeToJson(Type fieldTypeType, Object input)
        {
            var fieldType = (IFieldType)Activator.CreateInstance(fieldTypeType);
            Assert.Throws<TypeValidationException>(() => fieldType.FromJson(input));
        }
    }
}
