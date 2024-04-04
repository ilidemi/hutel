using System;
using System.Globalization;

namespace hutel.Logic
{
    public class HutelClock
    {
        public TimeSpan TimeSpan { get; }

        private const string _format = @"h\:mm";

        public HutelClock(string clock)
        {
            TimeSpan = TimeSpan.ParseExact(clock, _format, CultureInfo.InvariantCulture);
        }

        override public bool Equals(Object obj)
        {
            return obj is HutelClock && TimeSpan == ((HutelClock)obj).TimeSpan;
        }

        override public int GetHashCode()
        {
            return TimeSpan.GetHashCode();
        }

        override public string ToString()
        {
            return TimeSpan.ToString(_format, CultureInfo.InvariantCulture);
        }

        public static bool operator>(HutelClock a, HutelClock b)
        {
            return a.TimeSpan > b.TimeSpan;
        }

        public static bool operator<(HutelClock a, HutelClock b)
        {
            return a.TimeSpan < b.TimeSpan;
        }

        public static bool operator==(HutelClock a, HutelClock b)
        {
            return a.TimeSpan == b.TimeSpan;
        }

        public static bool operator!=(HutelClock a, HutelClock b)
        {
            return a.TimeSpan != b.TimeSpan;
        }

        public static bool operator>=(HutelClock a, HutelClock b)
        {
            return a.TimeSpan >= b.TimeSpan;
        }

        public static bool operator<=(HutelClock a, HutelClock b)
        {
            return a.TimeSpan <= b.TimeSpan;
        }
    }
}
