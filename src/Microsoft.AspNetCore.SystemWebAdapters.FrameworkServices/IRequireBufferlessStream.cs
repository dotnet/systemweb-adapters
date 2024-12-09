// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// Used to mark <see cref="IHttpHandler"/> instances that require bufferless stream for input in order
/// to perform streaming operations.
/// </summary>
internal interface IRequireBufferlessStream
{
}
