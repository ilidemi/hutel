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

        public BaseTagField(TagFieldDataContract tagFieldDataContract)
        {
            _name = tagFieldDataContract.Name;
        }

        public abstract Object ValueFromDataContract(Object obj);

        public abstract Object ValueToDataContract(Object obj);

        public virtual TagFieldDataContract ToDataContract()
        {
            return new TagFieldDataContract
            {
                Name = _name,
                Type = TypeString
            };
        }

        public static BaseTagField FromDataContract(TagFieldDataContract fieldDataContract)
        {
            if (fieldDataContract.Name == string.Empty)
            {
                throw new TagValidationException("Field name is empty");
            }
            Type type;
            if (!StringToFieldType.TryGetValue(fieldDataContract.Type, out type))
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
        public override string TypeString { get { return TagFieldConstants.Float; } }

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
        public override string TypeString { get { return TagFieldConstants.String; } }

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
        public override string TypeString { get { return TagFieldConstants.Date; } }

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
        public override string TypeString { get { return TagFieldConstants.Time; } }

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

        public EnumTagField(TagFieldDataContract fieldDataContract)
            : base(fieldDataContract)
        {
            if (fieldDataContract.Values == null || !fieldDataContract.Values.Any())
            {
                throw new ArgumentOutOfRangeException("Empty enum is useless");
            }
            _values = fieldDataContract.Values;
        }

        public override Object ValueFromDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            throwIfNotInCollection(obj);
            return obj;
        }

        public override Object ValueToDataContract(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            throwIfNotInCollection(obj);
            return obj;
        }

        public override TagFieldDataContract ToDataContract()
        {
            return new TagFieldDataContract
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
