// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.AspNetCore.Http.Features;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static class FeatureCollectionExtensions
{
    internal static TFeature GetRequired<TFeature>(this IFeatureCollection features)
    {
        if (features.Get<TFeature>() is TFeature feature)
        {
            return feature;
        }

        throw new InvalidOperationException($"Feature {typeof(TFeature)} is not available");
    }
}
