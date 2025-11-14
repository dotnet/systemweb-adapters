// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Windows.Win32.Foundation;
using Windows.Win32.Media.Audio;
using Windows.Win32.System.Com;

namespace Aspire.Hosting;

[SupportedOSPlatform("windows5.0")]
internal static partial class VisualStudioExtensions
{
    [LoggerMessage(LogLevel.Information, "Attempting to attach Visual Studio debugger to IIS process...")]
    private static partial void LogAttemptingToAttach(ILogger logger);

    [LoggerMessage(LogLevel.Information, "Attached Visual Studio debugger to IIS process (PID: {Pid}).")]
    private static partial void LogAttachedDebugger(ILogger logger, int pid);

    [LoggerMessage(LogLevel.Error, "Could not find IIS process (PID: {Pid}) in Visual Studio to attach debugger.")]
    private static partial void LogProcessNotFound(ILogger logger, int pid);

    [LoggerMessage(LogLevel.Error, "Could not determine IIS process ID to attach debugger.")]
    private static partial void LogPidNotDetermined(ILogger logger);

    [LoggerMessage(LogLevel.Error, "Failed to attach to IIS process for debugging.")]
    private static partial void LogAttachFailed(ILogger logger, Exception exception);

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to log output")]
    public static IResourceBuilder<IISExpressProjectResource> WithVisualStudioDebuggingSupport(this IResourceBuilder<IISExpressProjectResource> builder)
    {
        if (builder.ApplicationBuilder.ExecutionContext.IsRunMode && System.Diagnostics.Debugger.IsAttached)
        {
            builder.OnInitializeResource(async (iis, @event, token) =>
            {
                var notifications = @event.Services.GetRequiredService<ResourceNotificationService>();
                var logger = @event.Services.GetRequiredService<ResourceLoggerService>().GetLogger(iis);

                try
                {
                    if (await GetIISExpressPidAsync(notifications, iis.Name, token) is { } iisExpressPid)
                    {
                        LogAttemptingToAttach(logger);

                        if (await AttachIISExpressAsync(iisExpressPid))
                        {
                            LogAttachedDebugger(logger, iisExpressPid);
                            return;
                        }
                        else
                        {
                            LogProcessNotFound(logger, iisExpressPid);
                            return;
                        }
                    }
                    else
                    {
                        LogPidNotDetermined(logger);
                    }
                }
                catch (Exception e)
                {
                    LogAttachFailed(logger, e);
                }
            });
        }

        return builder;
    }

    private static async Task<int?> GetIISExpressPidAsync(ResourceNotificationService notifications, string resourceName, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (notifications.TryGetCurrentState(resourceName, out var notification))
                {
                    if (TryGetPid(notification.Snapshot.Properties, out var iisExpressPid))
                    {
                        return iisExpressPid;
                    }
                    else if (KnownResourceStates.TerminalStates.Contains(notification.Snapshot.State?.Text))
                    {
                        break;
                    }
                }

                await notifications.WaitForResourceAsync(resourceName, cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }

        return default;
    }

    private static bool TryGetPid(ImmutableArray<ResourcePropertySnapshot> properties, out int pid)
    {
        pid = 0;

        foreach (var property in properties)
        {
            if (property.Name == "executable.pid" && property.Value is int value)
            {
                pid = value;
            }
        }

        return pid != 0;
    }

    /// <summary>
    /// This method uses the DTE object model to manually attach the Visual Studio debugger to the IIS process. Aspire currently
    /// doesn't have an extension point to tell DCP to attach to a specific process, so we have to do it ourselves.
    /// </summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Need to capture all exceptions")]
    private static Task<bool> AttachIISExpressAsync(int iisExpressPid)
    {
        var tcs = new TaskCompletionSource<bool>();

        // Because we're accessing the DTE object model, we need to be in an STA thread with a message pump.
        var thread = new Thread(() =>
        {
            using var scheduler = new MessageFilter();

            try
            {
                tcs.SetResult(TryAttachDebugger(iisExpressPid));
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);

        thread.Start();
        thread.Join();

        return tcs.Task;
    }

    private static bool TryAttachDebugger(int iisPid)
    {
        var currentPid = Environment.ProcessId;

        // Go through all the existing Visual Studio instances
        foreach (var dte in GetVisualStudioInstances())
        {
            var debugger = dte.Debugger;

            // Check if the Aspire application is being debugged by it
            foreach (EnvDTE.Process debuggedProcess in debugger.DebuggedProcesses)
            {
                if (debuggedProcess.ProcessID == currentPid)
                {
                    // Go through the local processes the debugger can see and find the IIS one to attach
                    foreach (EnvDTE.Process dteProcess in debugger.LocalProcesses)
                    {
                        if (dteProcess.ProcessID == iisPid)
                        {
                            dteProcess.Attach();
                            return true;
                        }
                    }
                }
            }
        }

        return false;

        static IEnumerable<EnvDTE._DTE> GetVisualStudioInstances()
        {
            Windows.Win32.PInvoke.GetRunningObjectTable(0, out var runningObjectTable).ThrowOnFailure();
            runningObjectTable.EnumRunning(out var monikerEnumerator);
            monikerEnumerator.Reset();

            while (GetFirst(monikerEnumerator) is { } moniker)
            {
                runningObjectTable.GetObject(moniker, out var runningObjectVal);

                if (runningObjectVal is EnvDTE._DTE dte)
                {
                    yield return dte;
                }
            }

            static unsafe IMoniker? GetFirst(IEnumMoniker monikerEnumerator)
            {
                var monikers = new IMoniker[1];
                uint numFetched;
                if (monikerEnumerator.Next(1, monikers, &numFetched) == 0)
                {
                    return monikers[0];
                }

                return null;
            }
        }
    }

    // Based on: https://learn.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2010/ms228772(v=vs.100)?redirectedfrom=MSDN#example
    private sealed class MessageFilter : IMessageFilter, IDisposable
    {
        public MessageFilter()
        {
            Windows.Win32.PInvoke.CoRegisterMessageFilter(this, out _).ThrowOnFailure();
        }

        public void Dispose()
        {
            Windows.Win32.PInvoke.CoRegisterMessageFilter(null, out _).ThrowOnFailure();
        }

        unsafe uint IMessageFilter.HandleInComingCall(uint dwCallType, HTASK htaskCaller, uint dwTickCount, INTERFACEINFO_unmanaged* lpInterfaceInfo)
         => (uint)SERVERCALL.SERVERCALL_ISHANDLED;

        uint IMessageFilter.RetryRejectedCall(HTASK htaskCallee, uint dwTickCount, uint dwRejectType)
        {
            if (dwRejectType == (uint)SERVERCALL.SERVERCALL_RETRYLATER)
            {
                // Retry the thread call immediately if return >=0 && <100.
                return 99;
            }

            // Too busy; cancel call.
            return unchecked((uint)-1);
        }

        uint IMessageFilter.MessagePending(HTASK htaskCallee, uint dwTickCount, uint dwPendingType)
            => (uint)PENDINGMSG.PENDINGMSG_WAITDEFPROCESS;
    }
}
