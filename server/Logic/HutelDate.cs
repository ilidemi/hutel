using System;
using System.Globalization;

namespace hutel.Logic
{
    public class HutelDate
    {
        public DateTime DateTime { get; }

        private const string _format = "yyyy-MM-dd";

        public HutelDate(string date)
        {
            DateTime = DateTime.ParseExact(date, _format, CultureInfo.InvariantCulture);
        }

        override public bool Equals(Object obj)
        {
            return obj is HutelDate && DateTime == ((HutelDate)obj).DateTime;
        }

        override public int GetHashCode()
        {
            return DateTime.GetHashCode();
        }

        override public string ToString()
        {
            return DateTime.ToString(_format, CultureInfo.InvariantCulture);
        }

        public static bool operator>(HutelDate a, HutelDate b)
        {
            return a.DateTime > b.DateTime;
        }

        public static bool operator<(HutelDate a, HutelDate b)
        {
            return a.DateTime < b.DateTime;
        }

        public static bool operator==(HutelDate a, HutelDate b)
        {
            return a.DateTime == b.DateTime;
        }

        public static bool operator!=(HutelDate a, HutelDate b)
        {
            return a.DateTime != b.DateTime;
        }

        public static bool operator>=(HutelDate a, HutelDate b)
        {
            return a.DateTime >= b.DateTime;
        }

        public static bool operator<=(HutelDate a, HutelDate b)
        {
            return a.DateTime <= b.DateTime;
        }
    }
}
