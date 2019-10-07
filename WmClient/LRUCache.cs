/*
Copyright 2019 ScientiaMobile Inc. http://www.scientiamobile.com

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Wmclient
{
    public class LRUCache<TK, TV>
    {
        private const Int32 DEFAULT_SIZE = 10000;
        /// <summary>
        /// The dictionary to hash into any location in the list.
        /// </summary>
        internal readonly IDictionary<TK, Node> _keyToCacheEntryFirst;

        internal readonly Int32 cacheSize;

        private readonly Object _mutex;

        internal Node head;
        internal Node tail;

        public LRUCache()
            : this(DEFAULT_SIZE)
        {
        }

        public LRUCache(Int32 size)
        {
            if (size > 0)
                cacheSize = size;
            else
                cacheSize = DEFAULT_SIZE;

            _keyToCacheEntryFirst = new ConcurrentDictionary<TK, Node>();

            head = null;
            tail = null;

            _mutex = this;
        }

#region LRUCache<K,V> Members

        // First Level Cache Methods

        /// <summary>
        /// Gets the device for the specified key (user-agent).
        /// </summary>
        /// <param name="key">The useragent.</param>
        /// <returns>device</returns>
        public TV GetEntry(TK key)
        {
            lock (_mutex)
            {
                Node entry;

                if (!_keyToCacheEntryFirst.TryGetValue(key, out entry))
                {
                    return default(TV);
                }

                MoveToHead(entry);

                return entry.Value;
            }
        }

        /// <summary>
        /// Puts device in first level cache.
        /// </summary>
        /// <param name="key">A string key, be it user-agent, wurfl id or a concatenation of headers.</param>
        /// <param name="value">The device data.</param>
        /// <returns>device id</returns>
        public void PutEntry(TK key, TV value)
        {
            lock (_mutex)
            {
                Node entry;

                if (!_keyToCacheEntryFirst.TryGetValue(key, out entry))
                {
                    entry = new Node { Key = key, Value = value };

                    if (_keyToCacheEntryFirst.Count == cacheSize)
                    {
                        _keyToCacheEntryFirst.Remove(tail.Key);
                        tail = tail.Previous;
                        if (tail != null) tail.Next = null;
                    }
                    _keyToCacheEntryFirst.Add(key, entry);
                }

                entry.Value = value;
                MoveToHead(entry);
                if (tail == null) tail = head;

            }
        }

        public void Clear()
        {
            lock (_mutex)
            {
                _keyToCacheEntryFirst.Clear();
                head = null;
                tail = null;

            }
        }

        public int Size()
        {
            lock (_mutex)
            {
                return _keyToCacheEntryFirst.Count;
            }
        }

#endregion

        internal class Node
        {
            public Node Next { get; set; }
            public Node Previous { get; set; }
            public TK Key { get; set; }
            public TV Value { get; set; }
        }

        private void MoveToHead(Node entry)
        {
            if (entry == head || entry == null) return;

            var next = entry.Next;
            var previous = entry.Previous;

            if (next != null) next.Previous = entry.Previous;
            if (previous != null) previous.Next = entry.Next;

            entry.Previous = null;
            entry.Next = head;

            if (head != null) head.Previous = entry;
            head = entry;

            if (tail == entry) tail = previous;
        }
    }
}