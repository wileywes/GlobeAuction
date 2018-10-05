using System;
using System.Threading;

namespace GlobeAuction.Helpers
{
    public static class Utilities
    {
        public static void RetryIt(Action<int> function, string funcId, int maxAttempts)
        {
            for (int attempt=1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    function(attempt);
                    return;
                }
                catch(Exception exc)
                {
                    NLog.LogManager.GetCurrentClassLogger().Warn("Attempt {0} of retry call failed in {1}: {2}", attempt, funcId, exc);

                    Thread.Sleep(1000 * attempt);

                    if (attempt == maxAttempts)
                        throw;
                }
            }
        }

        public static DateTime GetEasternTimeNow()
        {
            var tzi = TimeZoneInfo.FindSystemTimeZoneById("US Eastern Standard Time");
            var isDaylightTime = tzi.IsDaylightSavingTime(DateTime.Today);
            var hoursDiff = isDaylightTime ? 4 : 5;
            var localTime = DateTime.UtcNow.AddHours(-1 * hoursDiff);
            return localTime;
        }
    }
}