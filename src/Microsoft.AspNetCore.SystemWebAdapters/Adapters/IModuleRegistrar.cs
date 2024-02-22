using System;

#if NETCOREAPP

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal interface IModuleRegistrar
{
    void RegisterModule(Type type, string? name = null);
}

#endif
