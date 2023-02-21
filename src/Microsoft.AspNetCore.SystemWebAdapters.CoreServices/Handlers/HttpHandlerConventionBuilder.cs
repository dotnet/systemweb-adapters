// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.Builder;

namespace Microsoft.AspNetCore.SystemWebAdapters;

public sealed class HttpHandlerConventionBuilder : IEndpointConventionBuilder
{
    private readonly IEndpointConventionBuilder _builder;

    internal HttpHandlerConventionBuilder(IEndpointConventionBuilder builder)
    {
        _builder = builder;
    }

    public void Add(Action<EndpointBuilder> convention)
        => _builder.Add(convention);

#if NET7_0_OR_GREATER
    public void Finally(Action<EndpointBuilder> finallyConvention)
        => _builder.Finally(finallyConvention);
#endif

}

