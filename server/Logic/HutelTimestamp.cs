using System;
using System.Globalization;

namespace hutel.Logic
{
    public class HutelTimestamp
    {
        public DateTime DateTime { get; }

        private const string _format = "yyyy-MM-dd HH:mm:ss";

        public HutelTimestamp(DateTime timestamp)
        {
            DateTime = timestamp;
        }

        public HutelTimestamp(string timestamp)
        {
            DateTime = DateTime.ParseExact(timestamp, _format, CultureInfo.InvariantCulture);
        }

        override public bool Equals(Object other)
        {
            return other is HutelTimestamp && DateTime == ((HutelTimestamp)other).DateTime;
        }

        override public int GetHashCode()
        {
            return DateTime.GetHashCode();
        }

        override public string ToString()
        {
            return DateTime.ToString(_format, CultureInfo.InvariantCulture);
        }

        public static bool operator>(HutelTimestamp a, HutelTimestamp b)
        {
            return a.DateTime > b.DateTime;
        }

        public static bool operator<(HutelTimestamp a, HutelTimestamp b)
        {
            return a.DateTime < b.DateTime;
        }

        public static bool operator==(HutelTimestamp a, HutelTimestamp b)
        {
            return a.DateTime == b.DateTime;
        }

        public static bool operator!=(HutelTimestamp a, HutelTimestamp b)
        {
            return a.DateTime != b.DateTime;
        }

        public static bool operator>=(HutelTimestamp a, HutelTimestamp b)
        {
            return a.DateTime >= b.DateTime;
        }

        public static bool operator<=(HutelTimestamp a, HutelTimestamp b)
        {
            return a.DateTime <= b.DateTime;
        }
    }
}
