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
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnumTagField(
                new TagFieldDataContract
                {
                    Name = EnumFieldName,
                    Type = TagFieldConstants.Enum,
                    Values =  new List<string>()
                }));
        }

        [Fact]
        public void ValidIntDataContract()
        {
            var intField = CreateIntField();
            var input = (Int64)0;
            Assert.Equal(intField.ValueFromDataContract(input), input);
            Assert.Equal(intField.ValueToDataContract(input), input);
        }

        [Fact]
        public void ValidFloatDataContract()
        {
            var floatField = CreateFloatField();
            var input = (Double)0.0;
            Assert.Equal(floatField.ValueFromDataContract(input), input);
            Assert.Equal(floatField.ValueToDataContract(input), input);
        }

        [Fact]
        public void ValidStringDataContract()
        {
            var stringField = CreateStringField();
            var input = "";
            Assert.Equal(stringField.ValueFromDataContract(input), input);
            Assert.Equal(stringField.ValueToDataContract(input), input);
        }

        [Fact]
        public void ValidDateDataContract()
        {
            var dateField = CreateDateField();
            var input = "2000-01-01";
            var date = dateField.ValueFromDataContract(input);
            var hutelDate = date as HutelDate;
            Assert.NotNull(hutelDate);
            Assert.Equal(hutelDate.DateTime, new DateTime(2000, 1, 1));
            Assert.Equal(dateField.ValueToDataContract(hutelDate), input);
        }

        public static IEnumerable<Object[]> ValidTimeDataContractData
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
        [MemberData(nameof(ValidTimeDataContractData))]
        public void ValidTimeDataContract(string input, TimeSpan expectedValue)
        {
            var timeField = CreateTimeField();
            var time = timeField.ValueFromDataContract(input);
            var hutelTime = time as HutelTime;
            Assert.NotNull(hutelTime);
            Assert.Equal(hutelTime.TimeSpan, expectedValue);
            Assert.Equal(timeField.ValueToDataContract(hutelTime), input);
        }

        [Fact]
        public void EnumFromDataContract()
        {
            var enumField = CreateEnumField();
            Assert.Equal(enumField.ValueFromDataContract(EnumValueA), EnumValueA);
            Assert.Equal(enumField.ValueFromDataContract(EnumValueB), EnumValueB);
            Assert.Throws<TypeValidationException>(() => enumField.ValueFromDataContract(0));
            Assert.Throws<TypeValidationException>(() => enumField.ValueFromDataContract(""));
            Assert.Throws<TypeValidationException>(() => enumField.ValueFromDataContract("c"));
        }

        [Fact]
        public void EnumToDataContract()
        {
            var enumField = CreateEnumField();
            Assert.Equal(enumField.ValueToDataContract(EnumValueA), EnumValueA);
            Assert.Equal(enumField.ValueToDataContract(EnumValueB), EnumValueB);
            Assert.Throws<TypeValidationException>(() => enumField.ValueToDataContract(0));
            Assert.Throws<TypeValidationException>(() => enumField.ValueToDataContract(""));
            Assert.Throws<TypeValidationException>(() => enumField.ValueToDataContract("c"));
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
        public void InvalidFieldFromDataContract(BaseTagField field, Object input)
        {
            Assert.Throws<TypeValidationException>(() => field.ValueFromDataContract(input));
        }
    }
}
