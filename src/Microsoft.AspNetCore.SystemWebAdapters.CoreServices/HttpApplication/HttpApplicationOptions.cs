// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Web;

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

    public IList<Type> Modules => _modules;

    internal void MakeReadOnly() => _modules.MakeReadOnly();

    /// <summary>
    /// Gets or sets the number of <see cref="HttpApplication"/> retained for reuse. In order to support modules and appplications that may contain state,
    /// a unique instance is required for each request. This value should be set to the average number of concurrent requests expected to be seen.
    /// </summary>
    public int PoolSize { get; set; } = 100;

    public void RegisterModule<T>()
         where T : IHttpModule
        => Modules.Add(typeof(T));

    /// <summary>
    /// A collection that validates that the types added are actual IHttpModule types
    /// </summary>
    private sealed class ModuleCollection : List<Type>, IList<Type>
    {
        private bool _isReadOnly;

        bool ICollection<Type>.IsReadOnly => _isReadOnly;

        public void MakeReadOnly() => _isReadOnly = true;

        Type IList<Type>.this[int index]
        {
            get => this[index];
            set
            {
                // We want to validate but we're replacing anything at this location so we want the duplicate check to skip that
                // Instead of calling the base indexer, we just remove the item at the index and insert it where it can do the checks we care about.
                RemoveAt(index);
                Insert(index, value);
            }
        }

        void IList<Type>.Insert(int index, Type item)
        {
            ValidateType(item);
            Insert(index, item);
        }

        void ICollection<Type>.Add(Type item)
        {
            ValidateType(item);
            Add(item);
        }

        public void CheckIsReadOnly()
        {
            if (_isReadOnly)
            {
                throw new InvalidOperationException("Module collection is readonly");
            }
        }

        private void ValidateType(Type type)
        {
            CheckIsReadOnly();

            if (Contains(type))
            {
                throw new InvalidOperationException($"Module {type.FullName} is already registered.");
            }

            if (!type.IsAssignableTo(typeof(IHttpModule)))
            {
                throw new InvalidOperationException($"Type {type.FullName} is not a valid IHttpModule.");
            }
        }
    }
}
