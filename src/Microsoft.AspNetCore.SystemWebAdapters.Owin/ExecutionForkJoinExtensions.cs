// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal static partial class ExecutionForkJoinExtensions
{
    /// <summary>
    /// Joins the forked pipeline back to the main pipeline execution.
    /// This method should be called from within the forked pipeline when it's ready to return control.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="key">The key identifying which fork point to join.</param>
    /// <returns>A task that completes when the main pipeline is ready to resume the forked execution.</returns>
    public static Task JoinPipelineFork(this HttpContext context, object key)
    {
        return context.Features.GetRequiredFeature<PipelineForkFeature>().WaitForPipelineJoin(key);
    }

    /// <summary>
    /// Forks the pipeline execution to run the specified delegate asynchronously,
    /// allowing the forked execution to join back to the main pipeline at a later point.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="key">A unique key identifying this fork point for later joining.</param>
    /// <param name="next">The request delegate to execute in the forked pipeline.</param>
    /// <returns>A task that completes when the forked pipeline joins back to the main pipeline.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Handled by request pipeline")]
    public static Task RunForkedPipelineAsync(this HttpContext context, object key, RequestDelegate next)
    {
        if (context.Features.Get<PipelineForkFeature>() is not PipelineForkFeature forkFeature)
        {
            forkFeature = new PipelineForkFeature(context, context.RequestServices.GetRequiredService<ILogger<PipelineForkFeature>>());
            context.Features.Set<PipelineForkFeature>(forkFeature);
            context.Response.OnCompleted(async state =>
            {
                var feature = (PipelineForkFeature)state;
                await feature.CompleteAsync();
            }, forkFeature);
        }

        return forkFeature.ForkPipeline(key, next);
    }

    private sealed partial class PipelineForkFeature(HttpContext context, ILogger logger)
    {
        internal Stack<(object Key, PipelineForkRunner Runner)> _forks { get; } = [];

        public async ValueTask CompleteAsync()
        {
            foreach (var (_, runner) in _forks)
            {
                await runner.CompleteAsync();
                runner.Dispose();
            }

            _forks.Clear();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Handled by containing class")]
        public Task ForkPipeline(object key, RequestDelegate next)
        {
            var runner = new PipelineForkRunner(context, next, logger);

            _forks.Push((key, runner));

            return runner.RunAsync();
        }

        public Task WaitForPipelineJoin(object key)
        {
            if (_forks.TryPeek(out var current))
            {
                if (!Equals(current.Key, key))
                {
                    throw new InvalidOperationException($"Invalid pipeline fork key {key}");
                }

                return current.Runner.JoinMainPipeline();
            }

            throw new InvalidOperationException($"No pipeline fork found.");
        }
    }

    private sealed partial class PipelineForkRunner : IDisposable
    {
        private readonly TaskCompletionSource _tcs1 = new();
        private readonly TaskCompletionSource _tcs2 = new();
        private readonly HttpContext _context;
        private readonly ILogger _logger;
        private readonly Task<Task> _initial;

        public PipelineForkRunner(HttpContext context, RequestDelegate next, ILogger logger)
        {
            _context = context;
            _logger = logger;
            _initial = new Task<Task>(() => next(context));
        }

        [LoggerMessage(Level = LogLevel.Information, Message = "Joining main pipeline from fork")]
        private partial void JoiningMainPipeline();

        [LoggerMessage(Level = LogLevel.Information, Message = "Resuming forked pipeline")]
        private partial void ResumingForkedPipeline();

        [LoggerMessage(Level = LogLevel.Information, Message = "Forked pipeline completed")]
        private partial void ForkedPipelineCompleted();

        public Task RunAsync()
        {
            _initial.Start();

            return _tcs1.Task;
        }

        public async Task JoinMainPipeline()
        {
            JoiningMainPipeline();
            _tcs1.SetResult();
            await _tcs2.Task;
            ResumingForkedPipeline();
        }

        public async Task CompleteAsync()
        {
            ResumingForkedPipeline();
            _tcs2.SetResult();
            await _initial.Unwrap();
            ForkedPipelineCompleted();
        }

        public void Dispose()
        {
            _tcs1.TrySetCanceled();
            _tcs2.TrySetCanceled();
            _initial.Dispose();
        }
    }
}
