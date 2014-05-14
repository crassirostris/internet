using System;
using System.Collections.Concurrent;

namespace DnsCache.DataStructures
{
    internal class TtlCache<TKey, TValue>
    {
        private class Record
        {
            public TValue Value { get; set; }
            public DateTime DeathTime { get; set; }
        }

        private readonly ConcurrentDictionary<TKey, Record> storage = new ConcurrentDictionary<TKey, Record>();

        public void Add(TKey key, TValue value, int ttl)
        {
            var record = new Record { Value = value, DeathTime = DateTime.Now + TimeSpan.FromSeconds(ttl) };
            storage[key] = record;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            value = default(TValue);
            Record record;
            if (!storage.TryGetValue(key, out record))
                return false;
            if (DateTime.Now >= record.DeathTime)
            {
                storage.TryRemove(key, out record);
                return false;
            }
            value = record.Value;
            return true;
        }
    }
}
