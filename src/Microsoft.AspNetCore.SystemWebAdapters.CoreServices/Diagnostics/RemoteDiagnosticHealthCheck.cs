using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.SystemWebAdapters.Diagnostics;

internal class RemoteDiagnosticHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _factory;
    private readonly IOptions<DiagnosticOptions> _options;
    private readonly IEnumerable<IClientDiagnostic> _diagnostics;

    public RemoteDiagnosticHealthCheck(IEnumerable<IClientDiagnostic> diagnostics, IOptions<DiagnosticOptions> options, IHttpClientFactory factory)
    {
        _factory = factory;
        _options = options;
        _diagnostics = diagnostics;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var client = _factory.CreateClient(RemoteConstants.HttpClientName);
        using var message = new HttpRequestMessage(HttpMethod.Get, _options.Value.Path);

        foreach (var diagnostic in _diagnostics)
        {
            diagnostic.Prepare(message);
        }

        using var response = await client.SendAsync(message, cancellationToken);

        var builder = new HealthCheckBuilder();

        foreach (var diagnostic in _diagnostics)
        {
            var result = diagnostic.Process(response);
            builder.Process(result);
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
