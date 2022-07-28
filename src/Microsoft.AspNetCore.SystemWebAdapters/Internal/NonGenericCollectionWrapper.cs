// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.SystemWebAdapters.Internal
{
    internal class NonGenericCollectionWrapper<T> : ICollection
    {
        private object? _syncRoot;
        private readonly ICollection<T> _collection;

        public NonGenericCollectionWrapper(ICollection<T> collection)
        {
            _collection = collection;
        }

        public int Count => _collection.Count;

        public bool IsSynchronized => (_collection as ICollection)?.IsSynchronized ?? false;

        public object SyncRoot => _syncRoot ??= (_collection as ICollection)?.SyncRoot ?? new object();

        public void CopyTo(Array array, int index)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            foreach (var item in _collection)
            {
                array.SetValue(item, index++);
            }
        }

        public IEnumerator GetEnumerator() => _collection.GetEnumerator();
    }
}
