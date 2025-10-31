// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static class OwinConstants
{
    internal const string StageAuthenticate = "Authenticate";
    internal const string StagePostAuthenticate = "PostAuthenticate";
    internal const string StageAuthorize = "Authorize";
    internal const string StagePostAuthorize = "PostAuthorize";
    internal const string StageResolveCache = "ResolveCache";
    internal const string StagePostResolveCache = "PostResolveCache";
    internal const string StageMapHandler = "MapHandler";
    internal const string StagePostMapHandler = "PostMapHandler";
    internal const string StageAcquireState = "AcquireState";
    internal const string StagePostAcquireState = "PostAcquireState";
    internal const string StagePreHandlerExecute = "PreHandlerExecute";

    internal const string BuilderDefaultApp = "builder.DefaultApp";

    internal const string IntegratedPipelineContext = "integratedpipeline.Context";
    internal const string IntegratedPipelineStageMarker = "integratedpipeline.StageMarker";
    internal const string IntegratedPipelineCurrentStage = "integratedpipeline.CurrentStage";

    internal const string OwinEnvironmentKey = "owin.Environment";
}
