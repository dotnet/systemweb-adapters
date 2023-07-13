// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1010 // Generic interface should also be implemented
#pragma warning disable CA1055 // URI-like return values should not be strings
#pragma warning disable CA1710 // Identifiers should have correct suffix
#pragma warning disable CA1008 // Enums should have zero value
#pragma warning disable CA1027 // Mark enums with FlagsAttribute
#pragma warning disable CA1069 // Enums values should not be duplicated
#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA1056 // URI-like properties should not be strings
#pragma warning disable CA1024 // Use properties where appropriate
#pragma warning disable CA1724 // Type names should not match namespaces
#pragma warning disable CA1716 // Using a reserved keyword as the name of a virtual/interface member makes it harder for consumers in other languages to override/implement the member.
#pragma warning disable CA1054 // URI parameters should not be strings
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize

namespace System.Web
{
    public partial class HttpApplication : System.IDisposable
    {
        public HttpApplication() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.Web.HttpApplicationState Application { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpContext Context { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpModuleCollection Modules { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpRequest Request { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpResponse Response { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpServerUtility Server { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.SessionState.HttpSessionState Session { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Security.Principal.IPrincipal User { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public event System.EventHandler AcquireRequestState { add { } remove { } }
        public event System.EventHandler AuthenticateRequest { add { } remove { } }
        public event System.EventHandler AuthorizeRequest { add { } remove { } }
        public event System.EventHandler BeginRequest { add { } remove { } }
        public event System.EventHandler Disposed { add { } remove { } }
        public event System.EventHandler EndRequest { add { } remove { } }
        public event System.EventHandler Error { add { } remove { } }
        public event System.EventHandler LogRequest { add { } remove { } }
        public event System.EventHandler MapRequestHandler { add { } remove { } }
        public event System.EventHandler PostAcquireRequestState { add { } remove { } }
        public event System.EventHandler PostAuthenticateRequest { add { } remove { } }
        public event System.EventHandler PostAuthorizeRequest { add { } remove { } }
        public event System.EventHandler PostLogRequest { add { } remove { } }
        public event System.EventHandler PostMapRequestHandler { add { } remove { } }
        public event System.EventHandler PostReleaseRequestState { add { } remove { } }
        public event System.EventHandler PostRequestHandlerExecute { add { } remove { } }
        public event System.EventHandler PostResolveRequestCache { add { } remove { } }
        public event System.EventHandler PostUpdateRequestCache { add { } remove { } }
        public event System.EventHandler PreRequestHandlerExecute { add { } remove { } }
        public event System.EventHandler PreSendRequestHeaders { add { } remove { } }
        public event System.EventHandler ReleaseRequestState { add { } remove { } }
        public event System.EventHandler ResolveRequestCache { add { } remove { } }
        public event System.EventHandler UpdateRequestCache { add { } remove { } }
        public void CompleteRequest() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Dispose() { }
        public virtual string GetVaryByCustomString(System.Web.HttpContext context, string custom) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public sealed partial class HttpApplicationState : System.Collections.Specialized.NameObjectCollectionBase
    {
        internal HttpApplicationState() { }
        public string[] AllKeys { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override int Count { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public object this[int index] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public object this[string name] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public void Add(string name, object value) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Clear() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public object Get(int index) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public object Get(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public string GetKey(int index) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Lock() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Remove(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void RemoveAll() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void RemoveAt(int index) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Set(string name, object value) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void UnLock() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpBrowserCapabilities : System.Web.Configuration.HttpCapabilitiesBase
    {
        internal HttpBrowserCapabilities() { }
    }
    public partial class HttpBrowserCapabilitiesBase
    {
        public HttpBrowserCapabilitiesBase() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual string Browser { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual bool Crawler { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual bool IsMobileDevice { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual int MajorVersion { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual double MinorVersion { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string Platform { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string Version { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
    }
    public partial class HttpBrowserCapabilitiesWrapper : System.Web.HttpBrowserCapabilitiesBase
    {
        public HttpBrowserCapabilitiesWrapper(System.Web.HttpBrowserCapabilities capabilities) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override string Browser { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override bool Crawler { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override bool IsMobileDevice { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override int MajorVersion { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override double MinorVersion { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string Platform { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string Version { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
    }
    public partial class HttpContext : System.IServiceProvider
    {
        internal HttpContext() { }
        public System.Exception[] AllErrors { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpApplicationState Application { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpApplication ApplicationInstance { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.Caching.Cache Cache { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public static System.Web.HttpContext Current { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.RequestNotification CurrentNotification { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Exception Error { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool IsDebuggingEnabled { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool IsPostNotification { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Collections.IDictionary Items { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpRequest Request { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpResponse Response { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpServerUtility Server { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.SessionState.HttpSessionState Session { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.DateTime Timestamp { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.TraceContext Trace { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Security.Principal.IPrincipal User { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public void AddError(System.Exception ex) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void ClearError() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.Web.ISubscriptionToken DisposeOnPipelineCompleted(System.IDisposable target) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void RewritePath(string path) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void RewritePath(string path, bool rebaseClientPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void RewritePath(string filePath, string pathInfo, string queryString) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void RewritePath(string filePath, string pathInfo, string queryString, bool setClientFilePath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        object System.IServiceProvider.GetService(System.Type service) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpContextBase : System.IServiceProvider
    {
        protected HttpContextBase() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual System.Exception[] AllErrors { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpApplicationState Application { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpApplication ApplicationInstance { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.Caching.Cache Cache { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.RequestNotification CurrentNotification { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Exception Error { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual bool IsDebuggingEnabled { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual bool IsPostNotification { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Collections.IDictionary Items { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpRequestBase Request { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpResponseBase Response { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpServerUtilityBase Server { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpSessionStateBase Session { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.DateTime Timestamp { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Security.Principal.IPrincipal User { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual void AddError(System.Exception ex) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void ClearError() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual object GetService(System.Type serviceType) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void RewritePath(string path) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void RewritePath(string path, bool rebaseClientPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void RewritePath(string filePath, string pathInfo, string queryString) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void RewritePath(string filePath, string pathInfo, string queryString, bool setClientFilePath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpContextWrapper : System.Web.HttpContextBase
    {
        public HttpContextWrapper(System.Web.HttpContext httpContext) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override System.Exception[] AllErrors { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpApplicationState Application { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpApplication ApplicationInstance { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.Caching.Cache Cache { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.RequestNotification CurrentNotification { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Exception Error { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override bool IsDebuggingEnabled { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override bool IsPostNotification { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Collections.IDictionary Items { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpRequestBase Request { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpResponseBase Response { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpServerUtilityBase Server { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpSessionStateBase Session { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.DateTime Timestamp { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Security.Principal.IPrincipal User { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override void AddError(System.Exception ex) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void ClearError() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void RewritePath(string path) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void RewritePath(string path, bool rebaseClientPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void RewritePath(string filePath, string pathInfo, string queryString) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void RewritePath(string filePath, string pathInfo, string queryString, bool setClientFilePath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public sealed partial class HttpCookie
    {
        public HttpCookie(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public HttpCookie(string name, string value) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public string Domain { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.DateTime Expires { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool HasKeys { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool HttpOnly { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string this[string key] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string Name { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string Path { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.SameSiteMode SameSite { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool Secure { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool Shareable { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string Value { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection Values { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
    }
    public sealed partial class HttpCookieCollection : System.Collections.Specialized.NameObjectCollectionBase
    {
        public HttpCookieCollection() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public string[] AllKeys { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpCookie this[int index] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpCookie this[string name] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public void Add(System.Web.HttpCookie cookie) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Clear() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.Web.HttpCookie Get(int index) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.Web.HttpCookie Get(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public string GetKey(int index) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Remove(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Set(System.Web.HttpCookie cookie) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpException : System.SystemException
    {
        public HttpException() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public HttpException(int httpStatusCode) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public HttpException(int httpStatusCode, string message) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public HttpException(int httpStatusCode, string message, System.Exception innerException) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public HttpException(System.Net.HttpStatusCode httpStatusCode) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public HttpException(System.Net.HttpStatusCode httpStatusCode, string message) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public HttpException(System.Net.HttpStatusCode httpStatusCode, string message, System.Exception innerException) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public HttpException(string message) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public HttpException(string message, System.Exception innerException) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public int StatusCode { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public int GetHttpCode() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public sealed partial class HttpFileCollection : System.Collections.Specialized.NameObjectCollectionBase
    {
        internal HttpFileCollection() { }
        public string[] AllKeys { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override int Count { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpPostedFile this[int index] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpPostedFile this[string name] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        [System.ObsoleteAttribute("Retrieving Keys is not supported on .NET 6+. Please use the enumerator instead.")]
        public override System.Collections.Specialized.NameObjectCollectionBase.KeysCollection Keys { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpPostedFile Get(int index) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.Web.HttpPostedFile Get(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override System.Collections.IEnumerator GetEnumerator() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.Collections.Generic.IList<System.Web.HttpPostedFile> GetMultiple(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public abstract partial class HttpFileCollectionBase : System.Collections.Specialized.NameObjectCollectionBase
    {
        protected HttpFileCollectionBase() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual string[] AllKeys { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpPostedFileBase this[int index] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpPostedFileBase this[string name] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpPostedFileBase Get(int index) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual System.Web.HttpPostedFileBase Get(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual System.Collections.Generic.IList<System.Web.HttpPostedFileBase> GetMultiple(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpFileCollectionWrapper : System.Web.HttpFileCollectionBase
    {
        public HttpFileCollectionWrapper(System.Web.HttpFileCollection httpFileCollection) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override string[] AllKeys { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override int Count { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpPostedFileBase this[int index] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpPostedFileBase this[string name] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpPostedFileBase Get(int index) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override System.Web.HttpPostedFileBase Get(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override System.Collections.IEnumerator GetEnumerator() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override System.Collections.Generic.IList<System.Web.HttpPostedFileBase> GetMultiple(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public sealed partial class HttpModuleCollection : System.Collections.Specialized.NameObjectCollectionBase
    {
        internal HttpModuleCollection() { }
        public string[] AllKeys { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.IHttpModule this[int index] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.IHttpModule this[string name] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public void CopyTo(System.Array dest, int index) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.Web.IHttpModule Get(int index) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.Web.IHttpModule Get(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public string GetKey(int index) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public sealed partial class HttpPostedFile
    {
        internal HttpPostedFile() { }
        public int ContentLength { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string ContentType { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string FileName { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.IO.Stream InputStream { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
    }
    public abstract partial class HttpPostedFileBase
    {
        protected HttpPostedFileBase() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual int ContentLength { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string ContentType { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string FileName { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.IO.Stream InputStream { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
    }
    public partial class HttpPostedFileWrapper : System.Web.HttpPostedFileBase
    {
        public HttpPostedFileWrapper(System.Web.HttpPostedFile httpPostedFile) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override int ContentLength { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string ContentType { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string FileName { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.IO.Stream InputStream { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
    }
    public partial class HttpRequest
    {
        internal HttpRequest() { }
        public string[] AcceptTypes { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string ApplicationPath { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string AppRelativeCurrentExecutionFilePath { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpBrowserCapabilities Browser { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Text.Encoding ContentEncoding { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public int ContentLength { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string ContentType { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpCookieCollection Cookies { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string FilePath { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpFileCollection Files { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection Form { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection Headers { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string HttpMethod { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.IO.Stream InputStream { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool IsAuthenticated { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool IsLocal { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool IsSecureConnection { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string this[string key] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Security.Principal.IIdentity LogonUserIdentity { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection Params { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string Path { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string PathInfo { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection QueryString { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string RawUrl { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.ReadEntityBodyMode ReadEntityBodyMode { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string RequestType { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection ServerVariables { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public int TotalBytes { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Uri Url { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Uri UrlReferrer { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string UserAgent { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string UserHostAddress { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string UserHostName { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string[] UserLanguages { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public void Abort() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public byte[] BinaryRead(int count) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.IO.Stream GetBufferedInputStream() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.IO.Stream GetBufferlessInputStream() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void SaveAs(string filename, bool includeHeaders) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public abstract partial class HttpRequestBase
    {
        protected HttpRequestBase() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual string[] AcceptTypes { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string ApplicationPath { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string AppRelativeCurrentExecutionFilePath { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpBrowserCapabilitiesBase Browser { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Text.Encoding ContentEncoding { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual int ContentLength { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string ContentType { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpCookieCollection Cookies { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpFileCollectionBase Files { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Collections.Specialized.NameValueCollection Form { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Collections.Specialized.NameValueCollection Headers { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string HttpMethod { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.IO.Stream InputStream { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual bool IsAuthenticated { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual bool IsLocal { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual bool IsSecureConnection { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string this[string key] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Security.Principal.IIdentity LogonUserIdentity { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Collections.Specialized.NameValueCollection Params { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string Path { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Collections.Specialized.NameValueCollection QueryString { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string RawUrl { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string RequestType { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Collections.Specialized.NameValueCollection ServerVariables { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual int TotalBytes { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Uri Url { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Uri UrlReferrer { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string UserAgent { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string UserHostAddress { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string UserHostName { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string[] UserLanguages { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual void Abort() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual byte[] BinaryRead(int count) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual System.IO.Stream GetBufferedInputStream() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual System.IO.Stream GetBufferlessInputStream() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpRequestWrapper : System.Web.HttpRequestBase
    {
        public HttpRequestWrapper(System.Web.HttpRequest request) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override string[] AcceptTypes { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string ApplicationPath { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string AppRelativeCurrentExecutionFilePath { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpBrowserCapabilitiesBase Browser { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Text.Encoding ContentEncoding { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override int ContentLength { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string ContentType { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpCookieCollection Cookies { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpFileCollectionBase Files { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Collections.Specialized.NameValueCollection Form { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Collections.Specialized.NameValueCollection Headers { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string HttpMethod { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.IO.Stream InputStream { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override bool IsAuthenticated { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override bool IsLocal { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override bool IsSecureConnection { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string this[string key] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Security.Principal.IIdentity LogonUserIdentity { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Collections.Specialized.NameValueCollection Params { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string Path { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Collections.Specialized.NameValueCollection QueryString { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string RawUrl { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string RequestType { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Collections.Specialized.NameValueCollection ServerVariables { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override int TotalBytes { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Uri Url { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Uri UrlReferrer { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string UserAgent { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string UserHostAddress { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string UserHostName { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string[] UserLanguages { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override void Abort() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override byte[] BinaryRead(int count) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override System.IO.Stream GetBufferedInputStream() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override System.IO.Stream GetBufferlessInputStream() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpResponse
    {
        internal HttpResponse() { }
        public string Charset { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Text.Encoding ContentEncoding { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string ContentType { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.HttpCookieCollection Cookies { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Collections.Specialized.NameValueCollection Headers { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool IsClientConnected { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool IsRequestBeingRedirected { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.IO.TextWriter Output { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.IO.Stream OutputStream { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string RedirectLocation { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public int StatusCode { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string StatusDescription { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public int SubStatusCode { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool SuppressContent { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool TrySkipIisCustomErrors { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public void AddHeader(string name, string value) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void AppendCookie(System.Web.HttpCookie cookie) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void AppendHeader(string name, string value) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void BinaryWrite(byte[] buffer) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Clear() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void ClearContent() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void ClearHeaders() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void End() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Flush() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.Threading.Tasks.Task FlushAsync() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Redirect(string url) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Redirect(string url, bool endResponse) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void RedirectPermanent(string url) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void RedirectPermanent(string url, bool endResponse) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void SetCookie(System.Web.HttpCookie cookie) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void TransmitFile(string filename) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void TransmitFile(string filename, long offset, long length) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Write(char ch) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Write(object obj) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Write(string s) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void WriteFile(string filename) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpResponseBase
    {
        public HttpResponseBase() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual string Charset { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Text.Encoding ContentEncoding { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string ContentType { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Web.HttpCookieCollection Cookies { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.Collections.Specialized.NameValueCollection Headers { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual bool IsClientConnected { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual bool IsRequestBeingRedirected { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.IO.TextWriter Output { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual System.IO.Stream OutputStream { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual int StatusCode { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string StatusDescription { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual int SubStatusCode { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual bool SuppressContent { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual bool TrySkipIisCustomErrors { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual void AddHeader(string name, string value) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void AppendCookie(System.Web.HttpCookie cookie) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void AppendHeader(string name, string value) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void BinaryWrite(byte[] buffer) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void Clear() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void ClearContent() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void ClearHeaders() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void End() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void SetCookie(System.Web.HttpCookie cookie) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void TransmitFile(string filename) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void TransmitFile(string filename, long offset, long length) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void Write(char ch) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void Write(object obj) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void Write(string s) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void WriteFile(string filename) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpResponseWrapper : System.Web.HttpResponseBase
    {
        public HttpResponseWrapper(System.Web.HttpResponse response) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override string Charset { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Text.Encoding ContentEncoding { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string ContentType { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Web.HttpCookieCollection Cookies { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.Collections.Specialized.NameValueCollection Headers { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override bool IsClientConnected { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override bool IsRequestBeingRedirected { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.IO.TextWriter Output { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override System.IO.Stream OutputStream { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override int StatusCode { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string StatusDescription { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override int SubStatusCode { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override bool SuppressContent { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override bool TrySkipIisCustomErrors { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override void AddHeader(string name, string value) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void AppendCookie(System.Web.HttpCookie cookie) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void AppendHeader(string name, string value) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void BinaryWrite(byte[] buffer) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void Clear() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void ClearContent() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void ClearHeaders() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void End() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void SetCookie(System.Web.HttpCookie cookie) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void TransmitFile(string filename) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void TransmitFile(string filename, long offset, long length) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void Write(char ch) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void Write(object obj) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void Write(string s) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void WriteFile(string filename) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public sealed partial class HttpRuntime
    {
        internal HttpRuntime() { }
        public static string AppDomainAppPath { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public static string AppDomainAppVirtualPath { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public static System.Web.Caching.Cache Cache { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
    }
    public partial class HttpServerUtility
    {
        internal HttpServerUtility() { }
        public string MachineName { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public void ClearError() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.Exception GetLastError() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public string MapPath(string path) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static byte[] UrlTokenDecode(string input) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static string UrlTokenEncode(byte[] input) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpServerUtilityBase
    {
        public HttpServerUtilityBase() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual string MachineName { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual void ClearError() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual System.Exception GetLastError() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual string MapPath(string path) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual byte[] UrlTokenDecode(string input) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual string UrlTokenEncode(byte[] input) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpServerUtilityWrapper : System.Web.HttpServerUtilityBase
    {
        public HttpServerUtilityWrapper(System.Web.HttpServerUtility utility) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override string MachineName { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override void ClearError() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override System.Exception GetLastError() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override string MapPath(string path) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override byte[] UrlTokenDecode(string input) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override string UrlTokenEncode(byte[] input) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public abstract partial class HttpSessionStateBase
    {
        protected HttpSessionStateBase() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual int Count { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual bool IsNewSession { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual bool IsReadOnly { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual object this[string name] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual string SessionID { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual int Timeout { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public virtual void Abandon() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void Add(string name, object value) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void Clear() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void Remove(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual void RemoveAll() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial class HttpSessionStateWrapper : System.Web.HttpSessionStateBase
    {
        public HttpSessionStateWrapper(System.Web.SessionState.HttpSessionState session) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override int Count { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override bool IsNewSession { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override bool IsReadOnly { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override object this[string name] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override string SessionID { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override int Timeout { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public override void Abandon() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void Add(string name, object value) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void Clear() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void Remove(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public override void RemoveAll() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public sealed partial class HttpUnhandledException : System.Web.HttpException
    {
        public HttpUnhandledException() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public HttpUnhandledException(string message) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public HttpUnhandledException(string message, System.Exception innerException) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial interface IHttpModule
    {
        void Dispose();
        void Init(System.Web.HttpApplication application);
    }
    public partial interface ISubscriptionToken
    {
        bool IsActive { get; }
        void Unsubscribe();
    }
    public enum ReadEntityBodyMode
    {
        Buffered = 3,
        Bufferless = 2,
        Classic = 1,
        None = 0,
    }
    [System.FlagsAttribute]
    public enum RequestNotification
    {
        AcquireRequestState = 32,
        AuthenticateRequest = 2,
        AuthorizeRequest = 4,
        BeginRequest = 1,
        EndRequest = 2048,
        ExecuteRequestHandler = 128,
        LogRequest = 1024,
        MapRequestHandler = 16,
        PreExecuteRequestHandler = 64,
        ReleaseRequestState = 256,
        ResolveRequestCache = 8,
        SendResponse = 536870912,
        UpdateRequestCache = 512,
    }
    public sealed partial class TraceContext
    {
        public TraceContext(System.Web.HttpContext context) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public bool IsEnabled { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public void Warn(string message) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Warn(string category, string message) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Warn(string category, string message, System.Exception errorInfo) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Write(string message) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Write(string category, string message) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Write(string category, string message, System.Exception errorInfo) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public static partial class VirtualPathUtility
    {
        public static string AppendTrailingSlash(string virtualPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static string Combine(string basePath, string relativePath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static string GetDirectory(string virtualPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static string GetExtension(string virtualPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static string GetFileName(string virtualPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static bool IsAbsolute(string virtualPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static bool IsAppRelative(string virtualPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static string MakeRelative(string fromPath, string toPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static string RemoveTrailingSlash(string virtualPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static string ToAbsolute(string virtualPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static string ToAbsolute(string virtualPath, string applicationPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static string ToAppRelative(string virtualPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public static string ToAppRelative(string virtualPath, string applicationPath) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
}
namespace System.Web.Caching
{
    public sealed partial class Cache : System.Collections.IEnumerable
    {
        public static readonly System.DateTime NoAbsoluteExpiration;
        public static readonly System.TimeSpan NoSlidingExpiration;
        public Cache() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public int Count { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public object this[string key] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public object Add(string key, object value, System.Web.Caching.CacheDependency dependencies, System.DateTime absoluteExpiration, System.TimeSpan slidingExpiration, System.Web.Caching.CacheItemPriority priority, System.Web.Caching.CacheItemRemovedCallback onRemoveCallback) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public object Get(string key) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.Collections.IEnumerator GetEnumerator() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Insert(string key, object value) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Insert(string key, object value, System.Web.Caching.CacheDependency dependencies) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Insert(string key, object value, System.Web.Caching.CacheDependency dependencies, System.DateTime absoluteExpiration, System.TimeSpan slidingExpiration) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Insert(string key, object value, System.Web.Caching.CacheDependency dependencies, System.DateTime absoluteExpiration, System.TimeSpan slidingExpiration, System.Web.Caching.CacheItemPriority priority, System.Web.Caching.CacheItemRemovedCallback onRemoveCallback) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Insert(string key, object value, System.Web.Caching.CacheDependency dependencies, System.DateTime absoluteExpiration, System.TimeSpan slidingExpiration, System.Web.Caching.CacheItemUpdateCallback onUpdateCallback) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public object Remove(string key) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public partial class CacheDependency : System.IDisposable
    {
        protected CacheDependency() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public CacheDependency(string filename) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public CacheDependency(string filename, System.DateTime start) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public CacheDependency(string[] filenames) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public CacheDependency(string[] filenames, System.DateTime start) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public CacheDependency(string[] filenames, string[] cachekeys) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public CacheDependency(string[] filenames, string[] cachekeys, System.DateTime start) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public CacheDependency(string[] filenames, string[] cachekeys, System.Web.Caching.CacheDependency dependency) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public CacheDependency(string[] filenames, string[] cachekeys, System.Web.Caching.CacheDependency dependency, System.DateTime start) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public bool HasChanged { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.DateTime UtcLastModified { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        protected virtual void DependencyDispose() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Dispose() { }
        protected virtual void Dispose(bool disposing) { }
        protected internal void FinishInit() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual string[] GetFileDependencies() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public virtual string GetUniqueID() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        protected void NotifyDependencyChanged(object sender, System.EventArgs e) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void SetCacheDependencyChanged(System.Action<object, System.EventArgs> dependencyChangedAction) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        protected void SetUtcLastModified(System.DateTime utcLastModified) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public enum CacheItemPriority
    {
        AboveNormal = 4,
        BelowNormal = 2,
        Default = 3,
        High = 5,
        Low = 1,
        Normal = 3,
        NotRemovable = 6,
    }
    public delegate void CacheItemRemovedCallback(string key, object value, System.Web.Caching.CacheItemRemovedReason reason);
    public enum CacheItemRemovedReason
    {
        DependencyChanged = 4,
        Expired = 2,
        Removed = 1,
        Underused = 3,
    }
    public delegate void CacheItemUpdateCallback(string key, System.Web.Caching.CacheItemUpdateReason reason, out object expensiveObject, out System.Web.Caching.CacheDependency dependency, out System.DateTime absoluteExpiration, out System.TimeSpan slidingExpiration);
    public enum CacheItemUpdateReason
    {
        DependencyChanged = 2,
        Expired = 1,
    }
}
namespace System.Web.Configuration
{
    public partial class HttpCapabilitiesBase
    {
        internal HttpCapabilitiesBase() { }
        public string Browser { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool Cookies { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool Crawler { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool IsMobileDevice { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string this[string key] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public int MajorVersion { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public double MinorVersion { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string Platform { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string PreferredRequestEncoding { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string Type { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string Version { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
    }
}
namespace System.Web.SessionState
{
    public partial class HttpSessionState : System.Collections.ICollection, System.Collections.IEnumerable
    {
        internal HttpSessionState() { }
        public int Count { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool IsNewSession { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool IsReadOnly { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public bool IsSynchronized { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public object this[string name] { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public System.Web.SessionState.SessionStateMode Mode { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public string SessionID { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public object SyncRoot { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public int Timeout { get { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} set { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");} }
        public void Abandon() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Add(string name, object value) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Clear() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void CopyTo(System.Array array, int index) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public System.Collections.IEnumerator GetEnumerator() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void Remove(string name) { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
        public void RemoveAll() { throw new System.PlatformNotSupportedException("Only supported when running on ASP.NET Core or System.Web");}
    }
    public enum SessionStateMode
    {
        Custom = 4,
        InProc = 1,
        Off = 0,
        SQLServer = 3,
        StateServer = 2,
    }
}
