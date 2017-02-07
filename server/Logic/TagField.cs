using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using hutel.Models;

namespace hutel.Logic
{
    public static class TagFieldConstants
    {
        public const string Int = "int";
        public const string Float = "float";
        public const string String = "string";
        public const string Date = "date";
        public const string Time = "time";
        public const string Enum = "enum";
    }

    public abstract class BaseTagField
    {
        public abstract string TypeString { get; }

        public string Name { get { return _name; } }

        public BaseTagField(TagFieldJson tagFieldJson)
        {
            _name = tagFieldJson.Name;
        }

        public abstract Object ValueFromJson(Object obj);

        public abstract Object ValueToJson(Object obj);

        public virtual TagFieldJson ToJson()
        {
            return new TagFieldJson
            {
                Name = _name,
                Type = TypeString
            };
        }

        public static BaseTagField FromJson(TagFieldJson tagFieldJson)
        {
            if (tagFieldJson.Name == string.Empty)
            {
                throw new TagValidationException("Field name is empty");
            }
            Type type;
            if (!StringToFieldType.TryGetValue(tagFieldJson.Type, out type))
            {
                throw new TagValidationException(
                    $"Unknown type {tagFieldJson.Type} in tag field {tagFieldJson.Name}");
            }
            try
            {
                return (BaseTagField)Activator.CreateInstance(type, tagFieldJson);
            }
            catch (Exception ex)
            {
                throw new TagValidationException(
                    $"Cannot initialize field {tagFieldJson.Name}", ex);
            }
        }

        protected readonly string _name;
        
        private static readonly Dictionary<string, Type> StringToFieldType =
            new Dictionary<string, Type>
            {
                { "int", typeof(IntTagField) },
                { "float", typeof(FloatTagField) },
                { "string", typeof(StringTagField) },
                { "date", typeof(DateTagField) },
                { "time", typeof(TimeTagField) },
                { "enum", typeof(EnumTagField) }
            };
    }

    public class IntTagField : BaseTagField
    {
        public override string TypeString { get { return TagFieldConstants.Int; } }

        public override Object ValueFromJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(Int64));
            return obj;
        }

        public override Object ValueToJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(Int64));
            return obj;
        }

        public IntTagField(TagFieldJson tagFieldJson) : base(tagFieldJson) {}
    }

    public class FloatTagField : BaseTagField
    {
        public override string TypeString { get { return TagFieldConstants.Float; } }

        public override Object ValueFromJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(Double));
            return obj;
        }

        public override Object ValueToJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(Double));
            return obj;
        }

        public FloatTagField(TagFieldJson tagFieldJson) : base(tagFieldJson) {}
    }

    public class StringTagField : BaseTagField
    {
        public override string TypeString { get { return TagFieldConstants.String; } }

        public override Object ValueFromJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            return obj;
        }

        public override Object ValueToJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            return obj;
        }

        public StringTagField(TagFieldJson tagFieldJson) : base(tagFieldJson) {}
    }

    public class DateTagField : BaseTagField
    {
        public override string TypeString { get { return TagFieldConstants.Date; } }

        public DateTagField(TagFieldJson tagFieldJson) : base(tagFieldJson) {}
        
        public override Object ValueFromJson(Object obj)
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

        public override Object ValueToJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(HutelDate));
            return ((HutelDate)obj).ToString();
        }
    }
    
    public class TimeTagField : BaseTagField
    {
        public override string TypeString { get { return TagFieldConstants.Time; } }

        public TimeTagField(TagFieldJson tagFieldJson) : base(tagFieldJson) {}
        
        public override Object ValueFromJson(Object obj)
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

        public override Object ValueToJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(HutelTime));
            return ((HutelTime)obj).ToString();
        }
    }

    public class EnumTagField : BaseTagField
    {
        public override string TypeString { get { return TagFieldConstants.Enum; } }

        public ICollection<string> Values
        {
            get
            {
                return _values;
            }
        }

        public EnumTagField(TagFieldJson tagFieldJson)
            : base(tagFieldJson)
        {
            if (tagFieldJson.Values == null || !tagFieldJson.Values.Any())
            {
                throw new ArgumentOutOfRangeException("Empty enum is useless");
            }
            _values = tagFieldJson.Values;
        }

        public override Object ValueFromJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            throwIfNotInCollection(obj);
            return obj;
        }

        public override Object ValueToJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            throwIfNotInCollection(obj);
            return obj;
        }

        public override TagFieldJson ToJson()
        {
            return new TagFieldJson
            {
                Name = _name,
                Type = TypeString,
                Values = _values.ToList()
            };
        }

        private void throwIfNotInCollection(object obj)
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

    public class TypeValidationHelper
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
