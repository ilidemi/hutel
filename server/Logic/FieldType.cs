using System;
using System.Collections.Generic;
using System.Reflection;

namespace hutel.Logic
{
    public interface IFieldType
    {
        Object FromJson(Object obj);

        Object ToJson(Object obj);
    }

    public class IntFieldType : IFieldType
    {
        public Object FromJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(Int64));
            return obj;
        }

        public Object ToJson(Object obj)
        {
            return obj;
        }
    }

    public class FloatFieldType : IFieldType
    {
        public Object FromJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(Double));
            return obj;
        }

        public Object ToJson(Object obj)
        {
            return obj;
        }
    }

    public class StringFieldType : IFieldType
    {
        public Object FromJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            return obj;
        }

        public Object ToJson(Object obj)
        {
            return obj;
        }
    }

    public class DateFieldType : IFieldType
    {
        public Object FromJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            return new HutelDate((string)obj);
        }

        public Object ToJson(Object obj)
        {
            return ((HutelDate)obj).ToString();
        }
    }
    
    public class TimeFieldType : IFieldType
    {
        public Object FromJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            return new HutelTime((string)obj);
        }

        public Object ToJson(Object obj)
        {
            return ((HutelTime)obj).ToString();
        }
    }

    public class EnumFieldType : IFieldType
    {
        private readonly IList<string> _values;

        public EnumFieldType(IList<string> values)
        {
            _values = values;
        }

        public Object FromJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            var str = (string)obj;
            if (!_values.Contains(str))
            {
                throw new TypeValidationException($"Invalid enum value: {str}");
            }
            return str;
        }

        public Object ToJson(Object obj)
        {
            return obj;
        }
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
