using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;

internal class SystemWebAdaptersHealthCheck : IHealthCheck
{
    private readonly IEnumerable<IDiagnostic> _diagnostics;

    public SystemWebAdaptersHealthCheck(IEnumerable<IDiagnostic> diagnostics)
    {
        _diagnostics = diagnostics;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var builder = new HealthCheckBuilder();

        foreach (var diagnostic in _diagnostics)
        {
            await foreach (var result in diagnostic.ProcessAsync(cancellationToken))
            {
                builder.Process(result);
            }
        }

        return builder.Build();
    }

    private class HealthCheckBuilder : Dictionary<string, object>
    {
        private HealthStatus _status = HealthStatus.Healthy;

        public void Process(DiagnosticResult result)
        {
            UpdateStatus(result.Status);

            Add(result.Name, result.Data);
        }

        private void UpdateStatus(DiagnosticStatus status)
        {
            if ((int)_status > (int)status)
            {
                _status = (HealthStatus)status;
            }
        }

        public HealthCheckResult Build() => new HealthCheckResult(_status, data: this);
    }
}
