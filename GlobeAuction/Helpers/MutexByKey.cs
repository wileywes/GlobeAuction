using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GlobeAuction.Helpers
{
    public class MutexByKey : IDisposable
    {
        private string _myKey;
        public bool WasAcquired { get; }
        private static ConcurrentDictionary<string, byte> _mutexLocks = new ConcurrentDictionary<string, byte>();

        private MutexByKey(string key, bool wasAcquired)
        {
            _myKey = key;
            WasAcquired = wasAcquired;
        }

        public static MutexByKey TryGetLock(string key)
        {
            var wasAcquired = _mutexLocks.TryAdd(key, 0x0);
            return new MutexByKey(key, wasAcquired);
        }

        public void Dispose()
        {
            if (WasAcquired)
            {
                byte val;
                _mutexLocks.TryRemove(_myKey, out val);
            }
        }
    }
}