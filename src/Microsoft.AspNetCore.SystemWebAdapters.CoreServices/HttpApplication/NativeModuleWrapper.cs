using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal partial class NativeModuleWrapper : IHttpModule
{
    private const string ModuleLoader = "Microsoft.AspNetCore.SystemWebAdapters.NativeModules.dll";

    private readonly nint _module;
    private readonly string _dll;
    private readonly List<RegistrationInfo> _registrations;
    private const uint RQ_BEGIN_REQUEST = 0x00000001;
    private const uint RQ_AUTHENTICATE_REQUEST = 0x00000002;
    private const uint RQ_AUTHORIZE_REQUEST = 0x00000004;
    private const uint RQ_RESOLVE_REQUEST_CACHE = 0x00000008;
    private const uint RQ_MAP_REQUEST_HANDLER = 0x00000010;
    private const uint RQ_ACQUIRE_REQUEST_STATE = 0x00000020;
    private const uint RQ_PRE_EXECUTE_REQUEST_HANDLER = 0x00000040;
    private const uint RQ_EXECUTE_REQUEST_HANDLER = 0x00000080;
    private const uint RQ_RELEASE_REQUEST_STATE = 0x00000100;
    private const uint RQ_UPDATE_REQUEST_CACHE = 0x00000200;
    private const uint RQ_LOG_REQUEST = 0x00000400;
    private const uint RQ_END_REQUEST = 0x00000800;

    public NativeModuleWrapper(string dll)
    {
        var path = Path.Combine(AppContext.BaseDirectory, dll);

        _registrations = new List<RegistrationInfo>();
        _dll = dll;
        _module = LoadNativeIISModule(path, new() { RegisterModule = ModuleRegistration });

        uint ModuleRegistration(IntPtr factory, uint moduleEvent, uint isPost)
        {
            _registrations.Add(new(factory, moduleEvent, isPost));
            return 0;
        }
    }

    public void Dispose()
    {
        if (_module is { } module)
        {
            UnloadNativeIISModule(_module);
        }
    }

    public void Init(HttpApplication application)
    {
        foreach (var (factory, moduleEvent, isPost) in _registrations)
        {
            // TODO: should use safehandles/etc to properly handle lifetimes
            var module = CreateModule(_module, factory);

            if (moduleEvent == RQ_BEGIN_REQUEST && isPost == 0)
            {
                application.BeginRequest += (s, o) =>
                {
                    var context = ((HttpApplication)application).Context;

                    // TODO: probably want to cache this for the life of the request
                    var c = CreateHttpContext((string name, string value) =>
                    {
                        context.Request.ServerVariables[name] = value;
                        return 0;
                    });

                    // TODO: what should the result be?
                    var result = CallEvent(module, c, moduleEvent, isPost);

                    DeleteHttpContext(c);
                };
            };
        }
    }

    private readonly record struct RegistrationInfo(IntPtr Factory, uint ModuleEvent, uint IsPost);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint RegisterModuleCallback(IntPtr factory, uint moduleEvent, uint isPost);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeModuleCallbacks
    {
        public RegisterModuleCallback RegisterModule;
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint SetServerVariableCallback([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPWStr)] string value);

    [DllImport(ModuleLoader, CharSet = CharSet.Unicode)]
    private static extern nint LoadNativeIISModule(string dll, NativeModuleCallbacks callbacks);

    [DllImport(ModuleLoader, CharSet = CharSet.Unicode)]
    private static extern nint UnloadNativeIISModule(nint module);

    [DllImport(ModuleLoader, CharSet = CharSet.Unicode)]
    private static extern nint CreateModule(nint module, nint factory);

    [DllImport(ModuleLoader, CharSet = CharSet.Unicode)]
    private static extern nint CreateHttpContext(SetServerVariableCallback setServer);

    [DllImport(ModuleLoader, CharSet = CharSet.Unicode)]
    private static extern void DeleteHttpContext(nint contex);

    [DllImport(ModuleLoader, CharSet = CharSet.Unicode)]
    private static extern int CallEvent(nint m, nint context, uint request, uint isPost);
}
