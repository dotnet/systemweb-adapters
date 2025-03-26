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
public class VerifyHttpBaseTypes(ITestOutputHelper output)
{
    private static readonly HashSet<(Type, string)> _skipped = new()
    {
        (typeof(HttpContext), "op_Implicit"),
        (typeof(HttpRequest), "op_Implicit"),
        (typeof(HttpResponse), "op_Implicit"),

        // These are members not found on HttpCachePolicyBase
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetCacheability)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.IsModified)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetNoServerCaching)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetVaryByCustom)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetCacheExtensions)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetNoTransforms)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetIgnoreRangeRequests)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetNoStore)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetExpires)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetMaxAge)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetProxyMaxAge)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.HasSlidingExpiration)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.IsValidUntilExpires)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetRevalidation)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetETag)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetUtcLastModified)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetLastModifiedFromFileDependencies)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetETagFromFileDependencies)),
        (typeof(HttpCachePolicy), nameof(HttpCachePolicy.GetOmitVaryStar)),
        (typeof(HttpCachePolicy), "get_UtcTimestampCreated"),
        (typeof(HttpCachePolicy), "set_UtcTimestampCreated"),
    };

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

            var found = baseType.GetMethod(method.Name, Flags, [.. method.GetParameters().Select(p => p.ParameterType)]);

            if (found is null)
            {
                output.WriteLine($"(typeof({type.Name}), nameof({type.Name}.{method.Name})),");
                isMissing = true;
            }
        }

        Assert.False(isMissing);
    }
}
