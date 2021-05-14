using System;

namespace Shared.Lib
{
    public static class DateTimeExtensions
    {
        public static long ToToUnixTimestamp(this DateTime MyDateTime)
        {
            //TimeSpan timeSpan = MyDateTime - new DateTime(1970, 1, 1, 0, 0, 0);
            //return (long)timeSpan.TotalSeconds*1000;
            return new DateTimeOffset(MyDateTime).ToUnixTimeSeconds() * 1000;
        }
    }
}
