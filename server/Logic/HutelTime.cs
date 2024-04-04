using System;
using System.Globalization;

namespace hutel.Logic
{
    public class HutelTime
    {
        public TimeSpan TimeSpan { get; }

        private const string _format = @"h\:mm\:ss";

        public HutelTime(string time)
        {
            TimeSpan = TimeSpan.ParseExact(time, _format, CultureInfo.InvariantCulture);
        }

        override public bool Equals(Object obj)
        {
            return obj is HutelTime && TimeSpan == ((HutelTime)obj).TimeSpan;
        }

        override public int GetHashCode()
        {
            return TimeSpan.GetHashCode();
        }

        override public string ToString()
        {
            return TimeSpan.ToString(_format, CultureInfo.InvariantCulture);
        }

        public static bool operator>(HutelTime a, HutelTime b)
        {
            return a.TimeSpan > b.TimeSpan;
        }

        public static bool operator<(HutelTime a, HutelTime b)
        {
            return a.TimeSpan < b.TimeSpan;
        }

        public static bool operator==(HutelTime a, HutelTime b)
        {
            return a.TimeSpan == b.TimeSpan;
        }

        public static bool operator!=(HutelTime a, HutelTime b)
        {
            return a.TimeSpan != b.TimeSpan;
        }

        public static bool operator>=(HutelTime a, HutelTime b)
        {
            return a.TimeSpan >= b.TimeSpan;
        }

        public static bool operator<=(HutelTime a, HutelTime b)
        {
            return a.TimeSpan <= b.TimeSpan;
        }
    }
}
