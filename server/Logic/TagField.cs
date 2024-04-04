using System;
using System.Collections.Generic;
using System.Linq;
using hutel.Models;

namespace hutel.Logic
{
    public static class TagFieldConstants
    {
        public const string IntType = "int";
        public const string FloatType = "float";
        public const string StringType = "string";
        public const string DateType = "date";
        public const string TimeType = "time";
        public const string ClockType = "clock";
        public const string EnumType = "enum";
    }

    public abstract class BaseTagField
    {
        public abstract string TypeString { get; }

        public string Name { get; }

        protected BaseTagField(TagFieldDataContract tagFieldDataContract)
        {
            Name = tagFieldDataContract.Name;
        }

        public abstract Object ValueFromDataContract(Object obj);

        public abstract Object ValueToDataContract(Object obj);

        public virtual TagFieldDataContract ToDataContract()
        {
            return new TagFieldDataContract
            {
                Name = Name,
                Type = TypeString
            };
        }

        public static BaseTagField FromDataContract(TagFieldDataContract fieldDataContract)
        {
            if (string.IsNullOrEmpty(fieldDataContract.Name))
            {
                throw new TagValidationException("Field name is empty");
            }
            if (!StringToFieldType.TryGetValue(fieldDataContract.Type, out Type type))
            {
                throw new TagValidationException(
                    $"Unknown type {fieldDataContract.Type} in tag field {fieldDataContract.Name}");
            }
            try
            {
                return (BaseTagField)Activator.CreateInstance(type, fieldDataContract);
            }
            catch (Exception ex)
            {
                throw new TagValidationException(
                    $"Cannot initialize field {fieldDataContract.Name}", ex);
            }
        }

        private static readonly Dictionary<string, Type> StringToFieldType =
            new Dictionary<string, Type>
            {
                { "int", typeof(IntTagField) },
                { "float", typeof(FloatTagField) },
                { "string", typeof(StringTagField) },
                { "date", typeof(DateTagField) },
                { "time", typeof(TimeTagField) },
                { "clock", typeof(ClockTagField) },
                { "enum", typeof(EnumTagField) }
            };
    }

    public class IntTagField : BaseTagField
    {
        public override string TypeString { get { return TagFieldConstants.IntType; } }

        public override Object ValueFromDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(Int64));
            return obj;
        }

        public override Object ValueToDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(Int64));
            return obj;
        }

        public IntTagField(TagFieldDataContract fieldDataContract) : base(fieldDataContract) {}
    }

    public class FloatTagField : BaseTagField
    {
        public override string TypeString { get { return TagFieldConstants.FloatType; } }

        public override Object ValueFromDataContract(Object obj)
        {
            var intObj = obj as Int64?;
            if (intObj != null)
            {
                return (Double)intObj;
            }
            TypeValidationHelper.Validate(obj, typeof(Double));
            return obj;
        }

        public override Object ValueToDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(Double));
            return obj;
        }

        public FloatTagField(TagFieldDataContract fieldDataContract) : base(fieldDataContract) {}
    }

    public class StringTagField : BaseTagField
    {
        public override string TypeString { get { return TagFieldConstants.StringType; } }

        public override Object ValueFromDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            return obj;
        }

        public override Object ValueToDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            return obj;
        }

        public StringTagField(TagFieldDataContract fieldDataContract) : base(fieldDataContract) {}
    }

    public class DateTagField : BaseTagField
    {
        public override string TypeString { get { return TagFieldConstants.DateType; } }

        public DateTagField(TagFieldDataContract fieldDataContract) : base(fieldDataContract) {}
        
        public override Object ValueFromDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            try
            {
                return new HutelDate((string)obj);
            }
            catch(Exception ex)
            {
                throw new TypeValidationException("Error in date constructor", ex);
            }
        }

        public override Object ValueToDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(HutelDate));
            return ((HutelDate)obj).ToString();
        }
    }
    
    public class TimeTagField : BaseTagField
    {
        public override string TypeString { get { return TagFieldConstants.TimeType; } }

        public TimeTagField(TagFieldDataContract fieldDataContract) : base(fieldDataContract) {}
        
        public override Object ValueFromDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            try
            {
                return new HutelTime((string)obj);
            }
            catch(Exception ex)
            {
                throw new TypeValidationException("Error in time constructor", ex);
            }
        }

        public override Object ValueToDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(HutelTime));
            return ((HutelTime)obj).ToString();
        }
    }
    
    public class ClockTagField : BaseTagField
    {
        public override string TypeString { get { return TagFieldConstants.ClockType; } }

        public ClockTagField(TagFieldDataContract fieldDataContract) : base(fieldDataContract) {}
        
        public override Object ValueFromDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            try
            {
                return new HutelClock((string)obj);
            }
            catch(Exception ex)
            {
                throw new TypeValidationException("Error in clock constructor", ex);
            }
        }

        public override Object ValueToDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(HutelClock));
            return ((HutelClock)obj).ToString();
        }
    }

    public class EnumTagField : BaseTagField
    {
        public override string TypeString { get { return TagFieldConstants.EnumType; } }

        public ICollection<string> Values
        {
            get
            {
                return _values;
            }
        }

        public EnumTagField(TagFieldDataContract fieldDataContract)
            : base(fieldDataContract)
        {
            if (fieldDataContract.Values == null || !fieldDataContract.Values.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(fieldDataContract), "Empty enum is useless");
            }
            _values = fieldDataContract.Values;
        }

        public override Object ValueFromDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            ThrowIfNotInCollection(obj);
            return obj;
        }

        public override Object ValueToDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            ThrowIfNotInCollection(obj);
            return obj;
        }

        public override TagFieldDataContract ToDataContract()
        {
            return new TagFieldDataContract
            {
                Name = Name,
                Type = TypeString,
                Values = _values.ToList()
            };
        }

        private void ThrowIfNotInCollection(object obj)
        {
            var str = (string)obj;
            if (!_values.Contains(str))
            {
                throw new TypeValidationException($"Invalid enum value: {str}");
            }
        }

        private readonly ICollection<string> _values;
    }
    
    public class TypeValidationException : Exception
    {
        public TypeValidationException()
        {
        }

        public TypeValidationException(string message): base(message)
        {
        }

        public TypeValidationException(string message, Exception inner) : base(message, inner)
        {
        }
    }

    public static class TypeValidationHelper
    {
        public static void Validate(Object obj, Type expectedType)
        {
            var objType = obj.GetType();
            if (!(expectedType.IsAssignableFrom(objType)))
            {
                throw new TypeValidationException(
                    $"Expected {expectedType.Name}, received {objType.Name} instead");
            }
        }
    }
}
