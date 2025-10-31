// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using static System.FormattableString;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public class HttpApplicationOptions
{
    private ModuleCollection? _modules;

    internal ModuleCollection ModuleCollection
    {
        get => _modules ?? throw new InvalidOperationException("HttpApplicationOptions must be initialized with a module collection");
        set => _modules = value;
    }

    internal Dictionary<ApplicationEvent, List<RequestDelegate>>? EventHandlers { get; private set; }

    /// <summary>
    /// Used to track if the services were added or if the options was just automatically created.
    /// </summary>
    internal bool IsAdded => _modules is { };

    private Type _applicationType = typeof(HttpApplication);

    public Type ApplicationType
    {
        get => _applicationType;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            ModuleCollection.CheckIsReadOnly();

            if (!_applicationType.IsAssignableTo(typeof(HttpApplication)))
            {
                throw new InvalidOperationException($"Type {value.FullName} is not a valid HttpApplication");
            }

            _applicationType = value;
        }
    }

    public IDictionary<string, Type> Modules => ModuleCollection;

    public void RegisterEvent(ApplicationEvent @event, RequestDelegate func)
    {
        EventHandlers ??= new();
        if (!EventHandlers.TryGetValue(@event, out var handlers))
        {
            handlers = new List<RequestDelegate>();
            EventHandlers[@event] = handlers;
        }
        handlers.Add(func);
    }

    /// <summary>
    /// Gets or sets whether <see cref="HttpApplication.PreSendRequestHeaders"/> and <see cref="HttpApplication.PreSendRequestContent"/> is supported
    /// </summary>
    public bool ArePreSendEventsEnabled { get; set; }

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

        ModuleCollection.RegisterModule(type, name);
    }
}
