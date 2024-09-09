using System;

namespace elasticsearch.Utils
{
    public static class DateConfiguration
    {
        public static DateTime GetBrazilianDateTime()
        {
            TimeZoneInfo brTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            DateTime brTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brTimeZone);

            return brTime;
        }

        public static TimeSpan GetTimeZoneOffset()
        {
            // Define o fuso horário para o Brasil
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
            return tz.GetUtcOffset(DateTime.Now);
        }
    }
}