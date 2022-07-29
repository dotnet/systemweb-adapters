// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.SessionState;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace System.Web;


public class SessionKeyTrackerOptions
{
    public Action<ISessionResultView>? OnRequest { get; set; }
}

public static class SessionKeyTrackerExtensions
{
    public static ISystemWebAdapterBuilder AddSessionKeyTracker(this ISystemWebAdapterBuilder builder, Action<SessionKeyTrackerOptions> options)
    {
        builder.Services.AddTransient<IHttpModule, SessionKeyTrackerModule>();
        builder.Services.AddOptions<SessionKeyTrackerOptions>()
            .Configure(options);

        return builder;
    }

    private class SessionKeyTrackerModule : IHttpModule
    {
        private const string SessionKey = "AspSession";
        private const string Tracker = "AspSessionTracker";

        private readonly IOptions<SessionKeyTrackerOptions> _options;

        public SessionKeyTrackerModule(IOptions<SessionKeyTrackerOptions> options)
        {
            _options = options;
        }

        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            if (_options.Value.OnRequest is not { } onRequest)
            {
                return;
            }

            context.PostAcquireRequestState += (s, e) =>
            {
                var context = ((HttpApplication)s).Context;

                if (SessionStateUtility.GetHttpSessionStateFromContext(context) is { } existing)
                {
                    var wrapped = new SessionTrackerWrapper(existing, context);
                    context.Items[Tracker] = wrapped;
                    ReplaceSessionState(context, wrapped);
                }

                // If an existing session exists, it will fail to add a new one
                static void ReplaceSessionState(HttpContext context, IHttpSessionState session)
                {
                    context.Items.Remove(SessionKey);
                    SessionStateUtility.AddHttpSessionStateToContext(context, session);
                }
            };

            context.EndRequest += (s, e) =>
            {
                var context = ((HttpApplication)s).Context;

                if (context.Items[Tracker] is SessionTrackerWrapper session)
                {
                    onRequest(session);
                }
            };
        }
    }

    private class SessionTrackerWrapper : ISessionResultView, IHttpSessionState
    {
        private static readonly IReadOnlyDictionary<string, Type> Empty = new Dictionary<string, Type>();

        private readonly IHttpSessionState _other;

        private bool _isDirty;
        private IReadOnlyDictionary<string, Type> _final;
        private List<string>? _accessed;

        public SessionTrackerWrapper(IHttpSessionState other, HttpContext context)
        {
            _other = other;
            HttpContext = new HttpContextWrapper(context);
            Initial = CreateSnapshot(other);
            _final = Initial;
        }

        private static IReadOnlyDictionary<string, Type> CreateSnapshot(IHttpSessionState state)
        {
            if (state.Count == 0)
            {
                return Empty;
            }

            var snapshot = new Dictionary<string, Type>(state.Count);

            foreach (string s in state.Keys)
            {
                if (state[s] is object o)
                {
                    snapshot.Add(s, o.GetType());
                }
            }

            return snapshot;
        }

        public HttpContextBase HttpContext { get; }

        public IReadOnlyCollection<string> AccessedKeys => ((IReadOnlyCollection<string>?)_accessed) ?? Array.Empty<string>();

        public IReadOnlyDictionary<string, Type> Initial { get; }

        public IReadOnlyDictionary<string, Type> Final
        {
            get
            {
                if (_isDirty)
                {
                    _final = CreateSnapshot(_other);
                    _isDirty = false;
                }

                return _final;
            }
        }

        private void AddAccessed(string name) => (_accessed ??= new()).Add(name);

        object IHttpSessionState.this[string name]
        {
            get
            {
                AddAccessed(name);
                return _other[name];
            }
            set
            {
                _isDirty = true;
                _other[name] = value;
            }
        }

        object IHttpSessionState.this[int index]
        {
            get => _other[index];
            set
            {
                _isDirty = true;
                _other[index] = value;
            }
        }

        string IHttpSessionState.SessionID => _other.SessionID;

        int IHttpSessionState.Timeout
        {
            get => _other.Timeout;
            set => _other.Timeout = value;
        }

        bool IHttpSessionState.IsNewSession => _other.IsNewSession;

        SessionStateMode IHttpSessionState.Mode => _other.Mode;

        bool IHttpSessionState.IsCookieless => _other.IsCookieless;

        HttpCookieMode IHttpSessionState.CookieMode => _other.CookieMode;

        int IHttpSessionState.LCID
        {
            get => _other.LCID;
            set => _other.LCID = value;
        }

        int IHttpSessionState.CodePage
        {
            get => _other.CodePage;
            set => _other.CodePage = value;
        }

        HttpStaticObjectsCollection IHttpSessionState.StaticObjects => _other.StaticObjects;

        int IHttpSessionState.Count => _other.Count;

        NameObjectCollectionBase.KeysCollection IHttpSessionState.Keys => _other.Keys;

        object IHttpSessionState.SyncRoot => _other.SyncRoot;

        bool IHttpSessionState.IsReadOnly => _other.IsReadOnly;

        bool IHttpSessionState.IsSynchronized => _other.IsSynchronized;

        void IHttpSessionState.Abandon() => _other.Abandon();

        void IHttpSessionState.Add(string name, object value)
        {
            _isDirty = true;
            _other.Add(name, value);
        }

        void IHttpSessionState.Clear()
        {
            _isDirty = true;
            _other.Clear();
        }

        void IHttpSessionState.CopyTo(Array array, int index) => _other.CopyTo(array, index);

        IEnumerator IHttpSessionState.GetEnumerator() => _other.GetEnumerator();

        void IHttpSessionState.Remove(string name)
        {
            _isDirty = true;
            _other.Remove(name);
        }

        void IHttpSessionState.RemoveAll()
        {
            _isDirty = true;
            _other.RemoveAll();
        }

        void IHttpSessionState.RemoveAt(int index)
        {
            _isDirty = true;
            _other.RemoveAt(index);
        }
    }
}

public interface ISessionResultView
{
    HttpContextBase HttpContext { get; }

    IReadOnlyDictionary<string, Type> Initial { get; }

    IReadOnlyDictionary<string, Type> Final { get; }

    IReadOnlyCollection<string> AccessedKeys { get; }
}


public class SessionKeyInfo
{
    public SessionKeyInfo(string key, Type type)
    {
        Key = key;
        Type = type;
    }

    public string Key { get; }

    public Type Type { get; }
}
