using System;
using System.Threading;

namespace GlobeAuction.Helpers
{
    public static class Utilities
    {
        public static void RetryIt(Action<int> function, string funcId, int maxAttempts)
        {
            for (int attempt=0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    attempt++;
                    function(attempt);
                    return;
                }
                catch(Exception exc)
                {
                    NLog.LogManager.GetCurrentClassLogger().Warn("Attempt {0} of retry call failed in {1}: {2}", attempt, funcId, exc);

                    Thread.Sleep(1000 * attempt);

                    if (attempt >= maxAttempts)
                        throw;
                }
            }
        }
    }
}