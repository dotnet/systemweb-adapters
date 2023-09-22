using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters.Blazor;

internal class HttpContextCircuitHandler : CircuitHandler
{
    private readonly Dictionary<string, HttpContext> _fakeHttpContext = new();

    public override Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        lock (_fakeHttpContext)
        {
            if (!_fakeHttpContext.TryGetValue(circuit.Id, out var context))
            {
                context = new DefaultHttpContext();
                var counter = new ReferenceCount();
                context.Features.Set(counter);
                context.Features.Set<IReferenceCounter>(counter);
                _fakeHttpContext.Add(circuit.Id, context);
            }
            else
            {
                context.Features.GetRequired<ReferenceCount>().Increment();
            }
        }

        var current = System.Web.HttpContext.Current;

        return Task.CompletedTask;
    }

    public override Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
    {
        lock (_fakeHttpContext)
        {
            if (_fakeHttpContext.TryGetValue(circuit.Id, out var context))
            {
                var counter = context.Features.GetRequired<ReferenceCount>();
                counter.Decrement();
                if (counter.Count == 0)
                {
                    _fakeHttpContext.Remove(circuit.Id);
                }
            }
        }
        return Task.CompletedTask;
    }

    private sealed class ReferenceCount : IReferenceCounter
    {
        private int _count = 1;

        public void Increment() => Interlocked.Increment(ref _count);

        public void Decrement() => Interlocked.Decrement(ref _count);

        public int Count => _count;
    }
}
