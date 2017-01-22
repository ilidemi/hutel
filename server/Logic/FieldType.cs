using System;
using System.Collections.Generic;
using System.Linq;
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
            TypeValidationHelper.Validate(obj, typeof(Int64));
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
            TypeValidationHelper.Validate(obj, typeof(Double));
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
            TypeValidationHelper.Validate(obj, typeof(string));
            return obj;
        }
    }

    public class DateFieldType : IFieldType
    {
        public Object FromJson(Object obj)
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

        public Object ToJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(HutelDate));
            return ((HutelDate)obj).ToString();
        }
    }
    
    public class TimeFieldType : IFieldType
    {
        public Object FromJson(Object obj)
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

        public Object ToJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(HutelTime));
            return ((HutelTime)obj).ToString();
        }
    }

    public class EnumFieldType : IFieldType
    {
        public ICollection<string> Values
        {
            get
            {
                return _values;
            }
        }

        private readonly ICollection<string> _values;

        public EnumFieldType(ICollection<string> values)
        {
            if (!values.Any())
            {
                throw new ArgumentOutOfRangeException("Empty enum is useless");
            }
            _values = values;
        }

        public Object FromJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            throwIfNotInCollection(obj);
            return obj;
        }

        public Object ToJson(Object obj)
        {
            TypeValidationHelper.Validate(obj, typeof(string));
            throwIfNotInCollection(obj);
            return obj;
        }

        private void throwIfNotInCollection(object obj)
        {
            var str = (string)obj;
            if (!_values.Contains(str))
            {
                throw new TypeValidationException($"Invalid enum value: {str}");
            }
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
