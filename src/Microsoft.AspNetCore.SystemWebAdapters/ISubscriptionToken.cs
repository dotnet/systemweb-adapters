// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Web;

public interface ISubscriptionToken
{
    /// <summary>
    /// Returns a value stating whether the subscription is currently active
    /// </summary>
    bool IsActive { get; }

    /// <summary>
    /// Unsubscribes from the event
    /// </summary>
    void Unsubscribe();
}
