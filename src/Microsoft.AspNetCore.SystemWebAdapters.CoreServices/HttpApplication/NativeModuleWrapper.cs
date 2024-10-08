using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.SystemWebAdapters;

internal unsafe partial class NativeModuleWrapper : IHttpModule
{
    private const string ModuleLoader = "Microsoft.AspNetCore.SystemWebAdapters.NativeModules.dll";

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

    private readonly nint _moduleLibrary;
    private readonly string _dll;
    private readonly List<RegistrationInfo> _registrations;

    private readonly NativeModuleCallbacks _callbacks;

    public NativeModuleWrapper(string dll)
    {
        var path = Path.Combine(AppContext.BaseDirectory, dll);

        _registrations = new List<RegistrationInfo>();
        _dll = dll;

        _callbacks = new()
        {
            RegisterModule = ModuleRegistration,
            HttpContextCallbacks = new()
            {
                setServerVariables = (pContext, name, value) =>
                {
                    var handle = GCHandle.FromIntPtr(pContext);

                    if (handle.Target is HttpContext context)
                    {
                        context.Request.ServerVariables[name] = value;
                        return 0;
                    }

                    // TODO error
                    return 1;
                },
            }
        };
        _moduleLibrary = LoadNativeIISModule(path, _callbacks);

        uint ModuleRegistration(IntPtr factory, uint moduleEvent, uint isPost)
        {
            _registrations.Add(new(factory, moduleEvent, isPost));
            return 0;
        }
    }

    public void Dispose()
    {
        if (_moduleLibrary is { } module)
        {
            UnloadNativeIISModule(_moduleLibrary);
        }
    }

    public void Init(HttpApplication application)
    {
        foreach (var (factory, moduleEvent, isPost) in _registrations)
        {
            // TODO: should use safehandles/etc to properly handle lifetimes
            var module = CreateModule(_moduleLibrary, factory);

            if (moduleEvent == RQ_BEGIN_REQUEST && isPost == 0)
            {
                application.BeginRequest += RegisterCallback;
            };

            void RegisterCallback(object? sender, EventArgs args)
            {
                var context = ((HttpApplication)application).Context;

                var h = GCHandle.Alloc(context);
                var result = CallEvent(_moduleLibrary, module, GCHandle.ToIntPtr(h), moduleEvent, isPost);
                h.Free();
            }
        }
    }

    private readonly record struct RegistrationInfo(IntPtr Factory, uint ModuleEvent, uint IsPost);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate uint RegisterModuleCallback(IntPtr factory, uint moduleEvent, uint isPost);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeModuleCallbacks
    {
        public RegisterModuleCallback RegisterModule;
        public NativeHttpContextCallbacks HttpContextCallbacks;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeHttpContextCallbacks
    {
        public SetServerVariableCallback setServerVariables;
    }

    private delegate uint SetServerVariableCallback(nint pContext, [MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPWStr)] string value);

    [DllImport(ModuleLoader, CharSet = CharSet.Unicode)]
    private static extern nint LoadNativeIISModule(string dll, in NativeModuleCallbacks callbacks);

    [DllImport(ModuleLoader, CharSet = CharSet.Unicode)]
    private static extern nint UnloadNativeIISModule(nint module);

    [DllImport(ModuleLoader, CharSet = CharSet.Unicode)]
    private static extern nint CreateModule(nint module, nint factory);

    [DllImport(ModuleLoader, CharSet = CharSet.Unicode)]
    private static extern int CallEvent(nint nativeModuleDll, nint m, nint context, uint request, uint isPost);
}
