// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if !NET7_0_OR_GREATER

using System;

namespace Microsoft.AspNetCore.Http.Features;

internal static class FeatureCollectionExtensions
{
    internal static TFeature GetRequiredFeature<TFeature>(this IFeatureCollection features)
    {
        if (features.Get<TFeature>() is TFeature feature)
        {
            return feature;
        }

        throw new InvalidOperationException($"Feature {typeof(TFeature)} is not available");
    }
}
#endif

