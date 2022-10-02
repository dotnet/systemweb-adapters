// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;

namespace System.Web.Caching;

public class CacheDependency
{
    private List<ChangeMonitor> changeMonitors = new();
    private bool hasChanged;
    private bool disposedValue;
    private DateTime utcLastModified;
    private Action<object, EventArgs>? dependencyChangedAction;
    private string[]? filenames;
    private readonly DateTime utcStart;
    private bool initCompleted;
    private string? uniqueId;
    private bool uniqueIdInitialized;

    internal CacheDependency()
    {
        FinishInit();
    }

    public CacheDependency(string filename) : this(filename, DateTime.MaxValue) { }

    public CacheDependency(string filename, DateTime start) : this(new[] { filename }, null, null, start) { }

    public CacheDependency(string[] filenames) : this(filenames, null, null, DateTime.MaxValue) { }

    public CacheDependency(string[] filenames, DateTime start) : this(filenames, null, null, start) { }

    public CacheDependency(string[]? filenames, string[]? cachekeys, DateTime start) :
        this(filenames, cachekeys, null, start)
    { }

    public CacheDependency(
        string[]? filenames,
        string[]? cachekeys,
        CacheDependency? dependency,
        DateTime start)
    {
        utcLastModified = DateTime.MinValue;
        if (start != DateTime.MaxValue && start.Kind != DateTimeKind.Utc)
        {
            start = start.ToUniversalTime();
        }
        utcStart = start;

        this.filenames = filenames;
        if (filenames is not null && filenames.Length != 0)
        {
            changeMonitors.Add(new HostFileChangeMonitor(filenames.ToList()));
        }

        if (cachekeys is not null && cachekeys.Length != 0)
        {
            changeMonitors.Add(HttpRuntime.Cache.ObjectCache
                                .CreateCacheEntryChangeMonitor(cachekeys));
        }

        if (dependency is not null)
        {
            changeMonitors.Add(dependency.GetChangeMonitor());
        }

        FinishInit();
    }

    protected internal void FinishInit()
    {
        hasChanged = changeMonitors.Any(cm => cm.HasChanged && (cm.GetLastModifiedUtc() > utcStart));
        utcLastModified = changeMonitors.Max(cm => cm.GetLastModifiedUtc());
        if (hasChanged)
        {
            NotifyDependencyChanged(this, EventArgs.Empty);
        }
        changeMonitors.ForEach(cm => cm.NotifyOnChanged(NotifyOnChanged));
        initCompleted = true;
    }

    private void NotifyOnChanged(object state) => NotifyDependencyChanged(this, new ChangeNotificationEventArgs(state));

    private class ChangeNotificationEventArgs : EventArgs
    {
        public ChangeNotificationEventArgs(object? state) => State = state;

        public object? State { get; }
    }

#pragma warning disable CA2109 // Review visible event handlers
    protected void NotifyDependencyChanged(object sender, EventArgs e)
#pragma warning restore CA2109 // Review visible event handlers
    {
        if (initCompleted && DateTime.UtcNow > utcStart)
        {
            hasChanged = true;
            utcLastModified = DateTime.UtcNow;
            dependencyChangedAction?.Invoke(sender, e);
        }
    }

    protected void SetUtcLastModified(DateTime utcLastModified) => this.utcLastModified = utcLastModified;

    public void SetCacheDependencyChanged(Action<object, EventArgs> dependencyChangedAction) =>
        this.dependencyChangedAction = dependencyChangedAction;

    public virtual string[] GetFileDependencies() => changeMonitors.OfType<FileChangeMonitor>().SelectMany(cm=>cm.FilePaths).ToArray();

    public bool HasChanged => hasChanged;

    public DateTime UtcLastModified => changeMonitors
        .OfType<FileChangeMonitor>()
        .Select(fcm => fcm.LastModified.DateTime)
        .Concat(new[] { utcLastModified })
        .Max();

    public virtual string? GetUniqueID()
    {
        if (!uniqueIdInitialized) {
            uniqueId = changeMonitors.Any(cm => cm.UniqueId is null) ?
                null :
                string.Join(":", changeMonitors.Select(cm => cm.UniqueId));
            uniqueIdInitialized = true;
        }
        return uniqueId;
    }


    #region "IDisposable"
    protected virtual void DependencyDispose() { }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                foreach (var changeMonitor in changeMonitors)
                {
                    changeMonitor?.Dispose();
                }
                changeMonitors.Clear();

                DependencyDispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion

    internal IEnumerable<ChangeMonitor> ChangeMonitors { get => changeMonitors; }
    internal ChangeMonitor GetChangeMonitor() => new CacheDependencyChangeMonitor(this);

    internal class CacheDependencyChangeMonitor : ChangeMonitor
    {
        private readonly CacheDependency cacheDependency;

        internal CacheDependencyChangeMonitor(CacheDependency cacheDependency)
        {
            this.cacheDependency = cacheDependency;
            cacheDependency.SetCacheDependencyChanged((state, _) => OnChanged(state));
            InitializationComplete();
        }

        public override string? UniqueId => cacheDependency.GetUniqueID();

        public DateTimeOffset LastModified => cacheDependency.UtcLastModified;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cacheDependency?.Dispose();
            }
        }
    }
}
