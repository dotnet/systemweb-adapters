using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Microsoft.AspNetCore.SystemWebAdapters.HttpApplication;

internal class NativeModuleWrapper(string dll) : IHttpModule
{
    private Load? _alc;

    public void Dispose()
    {
        _alc?.Unload();
    }

    public void Init(System.Web.HttpApplication application)
    {
        _alc = new Load(dll);
    }

    private sealed class Load : AssemblyLoadContext
    {
        private readonly IntPtr _ptr;

        public Load(string dll) : base(dll, isCollectible: true)
        {
            _ptr = LoadUnmanagedDll(dll);
        }
    }
}
