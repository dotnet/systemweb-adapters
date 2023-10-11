// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using Microsoft.AspNetCore.SystemWebAdapters.Features;
using Microsoft.AspNetCore.SystemWebAdapters.SessionState;

namespace System.Web.SessionState;

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = Constants.ApiFromAspNet)]
[Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1710:Identifiers should have correct suffix", Justification = Constants.ApiFromAspNet)]
public class HttpSessionState : ICollection
{
    private readonly Func<ISessionState> _state;

    internal HttpSessionState(ISessionStateFeature feature)
    {
        _state = () => feature.State ?? throw new InvalidOperationException("Session state is no longer available");
    }

    public HttpSessionState(ISessionState container)
    {
        _state = () => container;
    }

    internal ISessionState State => _state();

    public string SessionID => State.SessionID;

    public int Count => State.Count;

    public bool IsReadOnly => State.IsReadOnly;

    public bool IsNewSession => State.IsNewSession;

    public int Timeout
    {
        get => State.Timeout;
        set => State.Timeout = value;
    }

    public bool IsSynchronized => State.IsSynchronized;

    public object SyncRoot => State.SyncRoot;

    [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = Constants.ApiFromAspNet)]
    public SessionStateMode Mode => SessionStateMode.Custom;

    public void Abandon() => State.IsAbandoned = true;

    public object? this[string name]
    {
        get => State[name];
        set => State[name] = value;
    }

    public void Add(string name, object value) => State[name] = value;

    public void Remove(string name) => State.Remove(name);

    public void RemoveAll() => State.Clear();

    public void Clear() => State.Clear();

    public void CopyTo(Array array, int index)
    {
        ArgumentNullException.ThrowIfNull(array);

        foreach (var key in State.Keys)
        {
            array.SetValue(State[key], index++);
        }
    }

    public IEnumerator GetEnumerator() => State.Keys.GetEnumerator();
}
