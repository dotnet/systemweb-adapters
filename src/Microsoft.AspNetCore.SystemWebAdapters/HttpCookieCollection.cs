// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace System.Web;

[Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1010:Generic interface should also be implemented", Justification = Constants.ApiFromAspNet)]
public sealed class HttpCookieCollection : NameObjectCollectionBase
{
    public HttpCookieCollection()
    {
    }

    internal HttpCookieCollection(IRequestCookieCollection cookies)
    {
        foreach (var (name, value) in cookies)
        {
#pragma warning disable CA5396
            Add(new HttpCookie(name, value));
#pragma warning restore CA5396
        }
    }

    internal HttpCookieCollection(HttpResponse response)
    {
        response.AsAspNetCore().OnStarting(static state =>
        {
            var response = (HttpResponse)state;
            var headers = response.AsAspNetCore().Headers;
            var isShareable = false;

            for (var i = 0; i < response.Cookies.Count; i++)
            {
                if (response.Cookies[i] is { } cookie)
                {
                    headers.SetCookie = StringValues.Concat(headers.SetCookie, cookie.ToSetCookieHeaderValue().ToString());

                    isShareable |= cookie.Shareable;
                }
            }

            // We should suppress caching cookies if non-shareable cookies are
            // present in the response. Since these cookies can cary sensitive information, 
            // we should set Cache-Control: no-cache=set-cookie if there is such cookie
            // This prevents all well-behaved caches (both intermediary proxies and any local caches
            // on the client) from storing this sensitive information.
            // 
            // Additionally, we should not set this header during an SSL request, as certain versions
            // of IE don't handle it properly and simply refuse to render the page. More info:
            // http://blogs.msdn.com/b/ieinternals/archive/2009/10/02/internet-explorer-cannot-download-over-https-when-no-cache.aspx
            if (!isShareable && !response.AsAspNetCore().HttpContext.Request.IsHttps && response.TypedHeaders.CacheControl is { Public: true } cacheControl)
            {
                cacheControl.NoCache = true;
                cacheControl.NoCacheHeaders.Add(HeaderNames.SetCookie);

                response.TypedHeaders.CacheControl = cacheControl;
            }

            return Task.CompletedTask;
        }, response);
    }

    [Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = Constants.ApiFromAspNet)]
    public string?[] AllKeys => BaseGetAllKeys();

    public HttpCookie? this[string name] => Get(name);

    public HttpCookie? this[int index] => Get(index);

    public void Add(HttpCookie cookie)
    {
        ArgumentNullException.ThrowIfNull(cookie);

        BaseAdd(cookie.Name, cookie);
    }

    public void Set(HttpCookie cookie)
    {
        ArgumentNullException.ThrowIfNull(cookie);

        BaseSet(cookie.Name, cookie);
    }

    public HttpCookie? Get(string name) => (HttpCookie?)BaseGet(name);

    public HttpCookie? Get(int index) => (HttpCookie?)BaseGet(index);

    public string? GetKey(int index) => BaseGetKey(index);

    public void Remove(string name) => BaseRemove(name);

    public void Clear() => BaseClear();
}
