// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web;

using static System.FormattableString;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class HttpApplicationOptions
{
    private readonly ModuleCollection _modules = new();

    private Type _applicationType = typeof(HttpApplication);

    public Type ApplicationType
    {
        get => _applicationType;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _modules.CheckIsReadOnly();

            if (!_applicationType.IsAssignableTo(typeof(HttpApplication)))
            {
                throw new InvalidOperationException($"Type {value.FullName} is not a valid HttpApplication");
            }

            _applicationType = value;
        }
    }

    public IDictionary<string, Type> Modules => _modules;

    internal void MakeReadOnly() => _modules.MakeReadOnly();

    /// <summary>
    /// Gets or sets the number of <see cref="HttpApplication"/> retained for reuse. In order to support modules and applications that may contain state,
    /// a unique instance is required for each request. This type should be set to the average number of concurrent requests expected to be seen.
    /// </summary>
    public int PoolSize { get; set; } = 100;

    public void RegisterModule<T>(string? name = null)
         where T : IHttpModule
        => RegisterModule(typeof(T), name);

    public void RegisterModule(Type type, string? name = null)
    {
        ArgumentNullException.ThrowIfNull(type);

        Modules.Add(name ?? MakeUniqueModuleName(type), type);

        // Gets a dynamic name similar to how ASP.NET Framework did in the static HttpApplication.RegisterModule(Type moduleType) method
        static string MakeUniqueModuleName(Type type)
            => Invariant($"__DynamicModule_{type.AssemblyQualifiedName}_{Guid.NewGuid()}");
    }

    /// <summary>
    /// A collection that validates that the types added are actual IHttpModule types
    /// </summary>
    private sealed class ModuleCollection : IDictionary<string, Type>
    {
        private readonly Dictionary<string, Type> _inner;

        public ModuleCollection()
        {
            _inner = new(StringComparer.InvariantCultureIgnoreCase);
        }

        public Type this[string key]
        {
            get => _inner[key];
            set => _inner[key] = value;
        }

        public ICollection<string> Keys => _inner.Keys;

        public ICollection<Type> Values => _inner.Values;

        public int Count => _inner.Count;

        public bool IsReadOnly { get; private set; }

        public void Add(string key, Type type)
        {
            CheckIsReadOnly();

            if (Contains(new(key, type)))
            {
                throw new InvalidOperationException($"Module {type.FullName} is already registered with key '{key}'.");
            }

            if (!type.IsAssignableTo(typeof(IHttpModule)))
            {
                throw new InvalidOperationException($"Type {type.FullName} is not a valid IHttpModule.");
            }

            _inner.Add(key, type);
        }

        public void Add(KeyValuePair<string, Type> item) => Add(item.Key, item.Value);

        public void Clear()
        {
            CheckIsReadOnly();
            _inner.Clear();
        }

        public bool Contains(KeyValuePair<string, Type> item) => ((IDictionary<string, Type>)_inner).Contains(item);

        public bool ContainsKey(string key) => _inner.ContainsKey(key);

        public void CopyTo(KeyValuePair<string, Type>[] array, int arrayIndex) => ((ICollection<KeyValuePair<string, Type>>)_inner).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<string, Type>> GetEnumerator() => ((IEnumerable<KeyValuePair<string, Type>>)_inner).GetEnumerator();

        public void MakeReadOnly()
        {
            IsReadOnly = true;
        }

        public bool Remove(string key)
        {
            CheckIsReadOnly();
            return _inner.Remove(key);
        }

        public bool Remove(KeyValuePair<string, Type> item)
        {
            CheckIsReadOnly();
            return ((ICollection<KeyValuePair<string, Type>>)_inner).Remove(item);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out Type value) => _inner.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();

        public void CheckIsReadOnly()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException("Module collection is readonly");
            }
        }
    }
}
