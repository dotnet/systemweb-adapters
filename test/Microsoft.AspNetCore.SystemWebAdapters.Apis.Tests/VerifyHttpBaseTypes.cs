// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// This test validates that the *Base type (the abstraction in System.Web) has the same members as on the non-base type. For example, <see cref="HttpContext"/> and <see cref="HttpContextBase"/>.
/// This test is a best effort to ensure when an API is added to the main type, the base type gets it as well
/// </summary>
public class VerifyHttpBaseTypes
{
    private static readonly Dictionary<Type, string> _skipped = new()
    {
        [typeof(HttpContext)] = "op_Implicit",
        [typeof(HttpRequest)] = "op_Implicit",
        [typeof(HttpResponse)] = "op_Implicit",
        [typeof(HttpCachePolicy)] = nameof(HttpCachePolicy.GetCacheability),
    };

    private readonly ITestOutputHelper _output;

    public VerifyHttpBaseTypes(ITestOutputHelper output)
    {
        _output = output;
    }

    [InlineData(typeof(HttpContext), typeof(HttpContextBase))]
    [InlineData(typeof(HttpRequest), typeof(HttpRequestBase))]
    [InlineData(typeof(HttpResponse), typeof(HttpResponseBase))]
    [InlineData(typeof(HttpCachePolicy), typeof(HttpCachePolicyBase))]
    [InlineData(typeof(HttpPostedFile), typeof(HttpPostedFileBase))]
    [InlineData(typeof(HttpFileCollectionBase), typeof(HttpFileCollectionBase))]
    [InlineData(typeof(HttpServerUtilityBase), typeof(HttpServerUtilityBase))]
    [InlineData(typeof(HttpSessionStateBase), typeof(HttpSessionStateBase))]
    [Theory]
    public void ValidateMemberExistsOnBaseType(Type type, Type baseType)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(baseType);

        const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance;

        var isMissing = false;

        foreach (var method in type.GetMethods(Flags))
        {
            if (_skipped.Contains(new(type, method.Name)))
            {
                continue;
            }

            var found = baseType.GetMethod(method.Name, Flags, method.GetParameters().Select(p => p.ParameterType).ToArray());

            if (found is null)
            {
                _output.WriteLine(method.Name);
                isMissing = true;
            }
        }

        Assert.False(isMissing);
    }
}
