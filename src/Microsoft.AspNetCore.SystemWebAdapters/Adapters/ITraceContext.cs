using System;

#if NETCOREAPP

namespace Microsoft.AspNetCore.SystemWebAdapters;

public interface ITraceContext
{
    void Write(string message, string? category, Exception? errorInfo);

    void Warn(string message, string? category, Exception? errorInfo);
}

#endif
