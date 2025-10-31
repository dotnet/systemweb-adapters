// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Xunit;

namespace Microsoft.AspNetCore.SystemWebAdapters;

/// <summary>
/// Used to disable parallelization on tests that are hosted since the hosting environment is stored as a static property.
/// </summary>
[CollectionDefinition(nameof(SelfHostedTests), DisableParallelization = true)]
public class SelfHostedTests
{
}
