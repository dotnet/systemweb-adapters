// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.AspNetCore.SystemWebAdapters.SessionState.Serialization;

internal class ImmutableUnknownKeyTracker : IUnknownKeyTracker
{
    private ImmutableDictionary<string, ImmutableHashSet<Type>> _unknownTypes = ImmutableDictionary<string, ImmutableHashSet<Type>>.Empty;
    private ImmutableHashSet<string> _unknownKeys = ImmutableHashSet<string>.Empty;

    public ILookup<string, Type> UnknownTypes => new Wrapper(_unknownTypes);

    public IReadOnlyCollection<string> UnknownKeys => _unknownKeys;

    public void Add(IEnumerable<string> keys)
    {
        ImmutableInterlocked.Update(ref _unknownKeys, static (set, keys) =>
        {
            var builder = set.ToBuilder();

            foreach (var key in keys)
            {
                builder.Add(key);
            }

            return builder.ToImmutable();
        }, keys);
    }

    public void Add(string key, Type type)
    {
        ImmutableInterlocked.Update(ref _unknownKeys, static (set, key) => set.Add(key), key);
        ImmutableInterlocked.Update(ref _unknownTypes, static (u, arg) =>
        {
            var (key, type) = arg;

            if (u.TryGetValue(key, out var before))
            {
                var after = before.Add(type);

                if (before == after)
                {
                    return u;
                }

                return u.SetItem(key, after);
            }
            else
            {
                return u.Add(key, ImmutableHashSet.Create(type));
            }
        }, (key, type));
    }

    private class Wrapper : ILookup<string, Type>
    {
        public ImmutableDictionary<string, ImmutableHashSet<Type>> _unknown;

        public Wrapper(ImmutableDictionary<string, ImmutableHashSet<Type>> unknown)
        {
            _unknown = unknown;
        }

        public IEnumerable<Type> this[string key] => _unknown.TryGetValue(key, out var result) ? result : Enumerable.Empty<Type>();

        public int Count => _unknown.Count;

        public bool Contains(string key) => _unknown.ContainsKey(key);

        public IEnumerator<IGrouping<string, Type>> GetEnumerator()
        {
            foreach (var item in _unknown)
            {
                yield return new Grouping(item.Key, item.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class Grouping : IGrouping<string, Type>
        {
            private readonly ImmutableHashSet<Type> _types;

            public Grouping(string key, ImmutableHashSet<Type> types)
            {
                Key = key;
                _types = types;
            }

            public string Key { get; }

            public IEnumerator<Type> GetEnumerator() => _types.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => _types.GetEnumerator();
        }
    }
}
