using Microsoft.AspNetCore.Routing.Matching;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
internal sealed class SampleVisibleMode : Attribute
{
    public SampleMode App { get; set; }

    public bool Include { get; set; } = true;
}

internal enum SampleMode
{
    Remote,
    Owin,
}

sealed class SamplesPolicy(SampleMode sampleMode) : MatcherPolicy, IEndpointSelectorPolicy
{
    public override int Order => 0;

    public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        => endpoints.Any(ShouldSkip);

    private bool ShouldSkip(Endpoint e)
        => e.Metadata.GetMetadata<SampleVisibleMode>() is { App: { } app, Include: false } && app == sampleMode;

    public async Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
    {
        for (int i = 0; i < candidates.Count; i++)
        {
            if (ShouldSkip(candidates[i].Endpoint))
            {
                candidates.SetValidity(i, false);
            }
        }
    }
}
